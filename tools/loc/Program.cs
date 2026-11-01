using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LocTool;

/// <summary>
/// The game's texts for translators. Run from the repository root:
///   dotnet run --project tools/loc -- extract     writes locale/keywright.pot from every Loc.T, Loc.Plural and Loc.Key
///   dotnet run --project tools/loc -- check       lists texts the player reads that aren't wrapped yet (exit 1 if any)
///   dotnet run --project tools/loc -- survey      every unwrapped string by where it's used, to decide the rules
///   dotnet run --project tools/loc -- wrap [file]  wraps them in Loc.T (once, in 0.27; check finds stragglers)
/// A text's msgid is what Loc.T looks up: the literal, or an interpolated string with its holes numbered ("Day {0}").
/// </summary>
static class Program
{
    static int Main(string[] args)
    {
        string root = FindRoot();
        string command = args.FirstOrDefault() ?? "help";
        var only = args.Skip(1).Where(a => !a.StartsWith("--")).Select(a => Path.GetFullPath(a)).ToHashSet();
        var game = Game.Load(root);
        switch (command)
        {
            case "extract":
                return Extractor.Run(game, root);
            case "survey":
                return Survey.Run(game, verbose: args.Contains("--all"));
            case "check":
                return Survey.Check(game);
            case "wrap":
                return Wrapper.Run(game, only);
            default:
                Console.WriteLine("usage: dotnet run --project tools/loc -- extract | check | survey [--all] | wrap [files]");
                return 2;
        }
    }

    static string FindRoot()
    {
        for (var dir = new DirectoryInfo(Directory.GetCurrentDirectory()); dir != null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "project.godot"))) return dir.FullName;
        throw new InvalidOperationException("run it inside the repository (no project.godot found)");
    }
}

/// <summary>The game's sources, parsed and bound (with GodotSharp and the .NET libraries, so types are known).</summary>
sealed class Game
{
    public string Root;
    public CSharpCompilation Compilation;
    public List<SyntaxTree> Trees = new();

    /// <summary>Files whose strings are never the player's: debugging and the sim, the language layer itself (the
    /// languages' own names stay as they are).</summary>
    public bool Translated(SyntaxTree tree)
    {
        string rel = Rel(tree.FilePath);
        return !rel.StartsWith("scripts/Debug/") && rel is not ("scripts/Core/Loc.cs" or "scripts/Localization.cs" or "scripts/Storage.cs");
    }

    public string Rel(string path) => Path.GetRelativePath(Root, path).Replace('\\', '/');

    public static Game Load(string root)
    {
        var game = new Game { Root = root };
        var options = new CSharpParseOptions(LanguageVersion.CSharp12);
        foreach (string file in Directory.GetFiles(Path.Combine(root, "scripts"), "*.cs", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal))
            game.Trees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(file), options, file, Encoding.UTF8));
        var refs = new List<MetadataReference>();
        string runtime = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        foreach (string dll in Directory.GetFiles(runtime, "*.dll"))
        {
            try { refs.Add(MetadataReference.CreateFromFile(dll)); } catch { /* not a managed assembly */ }
        }
        string nuget = Environment.GetEnvironmentVariable("NUGET_PACKAGES") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
        foreach (string godot in Directory.GetFiles(Path.Combine(nuget, "godotsharp"), "GodotSharp.dll", SearchOption.AllDirectories))
        {
            refs.Add(MetadataReference.CreateFromFile(godot));
            break;
        }
        game.Compilation = CSharpCompilation.Create("Keywright", game.Trees, refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
        return game;
    }
}

/// <summary>A string expression in the code: a literal, an interpolated string, or a + of them with other things.</summary>
sealed class Found
{
    public ExpressionSyntax Node;
    public SyntaxTree Tree;
    /// <summary>Its text with holes numbered, as a translator would see it.</summary>
    public string Text;
    /// <summary>Where it's used: "MakeLabel(0)", "Name =", "return Describe", "compare"...</summary>
    public string Context;
    public string Verdict;
    public int Line => Tree.GetLineSpan(Node.Span).StartLinePosition.Line + 1;
}

static class Strings
{
    /// <summary>The outermost string expressions in a file that aren't already inside a Loc call.</summary>
    public static IEnumerable<Found> In(Game game, SyntaxTree tree)
    {
        var model = game.Compilation.GetSemanticModel(tree);
        foreach (var node in tree.GetRoot().DescendantNodes())
        {
            if (node is not ExpressionSyntax e || !IsStringExpression(e, model)) continue;
            // The whole of a + chain, not its parts (brackets and all).
            var up = e.Parent;
            while (up is ParenthesizedExpressionSyntax) up = up.Parent;
            if (up is BinaryExpressionSyntax { RawKind: (int)SyntaxKind.AddExpression } parent && IsStringExpression(parent, model)) continue;
            if (InsideLoc(e)) continue;
            string text = Template(e, model, out bool literal);
            if (!literal) continue;
            yield return new Found { Node = e, Tree = tree, Text = text, Context = Rules.Context(e, model) };
        }
    }

    static bool IsStringExpression(ExpressionSyntax e, SemanticModel model)
    {
        switch (e)
        {
            case LiteralExpressionSyntax { RawKind: (int)SyntaxKind.StringLiteralExpression }:
            case InterpolatedStringExpressionSyntax:
                return true;
            case BinaryExpressionSyntax { RawKind: (int)SyntaxKind.AddExpression } b:
                if (!ContainsLiteral(b)) return false;
                var type = model.GetTypeInfo(b).Type;
                return type?.SpecialType == SpecialType.System_String || (type == null || type.TypeKind == TypeKind.Error) && (IsStringExpression(b.Left, model) || IsStringExpression(b.Right, model));
            default:
                return false;
        }
    }

    /// <summary>A + chain with a literal of its own on its spine (not one buried in a call inside it).</summary>
    static bool ContainsLiteral(BinaryExpressionSyntax b)
    {
        foreach (var side in new[] { b.Left, b.Right })
        {
            var s = side is ParenthesizedExpressionSyntax p ? p.Expression : side;
            if (s is LiteralExpressionSyntax { RawKind: (int)SyntaxKind.StringLiteralExpression } or InterpolatedStringExpressionSyntax) return true;
            if (s is BinaryExpressionSyntax { RawKind: (int)SyntaxKind.AddExpression } inner && ContainsLiteral(inner)) return true;
        }
        return false;
    }

    public static bool InsideLoc(SyntaxNode node)
    {
        for (var n = node.Parent; n != null; n = n.Parent)
        {
            if (n is InvocationExpressionSyntax inv && IsLoc(inv)) return true;
            if (n is MemberDeclarationSyntax or StatementSyntax) return false;
        }
        return false;
    }

    public static bool IsLoc(InvocationExpressionSyntax inv) =>
        inv.Expression is MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Text: "Loc" } };

    /// <summary>
    /// The text as a translator sees it: literals as they are, each hole numbered in order ("{0}"). Literal is false
    /// when there's no literal text in it at all (a + of variables).
    /// </summary>
    public static string Template(ExpressionSyntax e, SemanticModel model, out bool literal)
    {
        var sb = new StringBuilder();
        int holes = 0;
        bool any = false;
        void Walk(ExpressionSyntax x)
        {
            switch (x)
            {
                case LiteralExpressionSyntax { RawKind: (int)SyntaxKind.StringLiteralExpression } lit:
                    sb.Append(lit.Token.ValueText);
                    any = true;
                    break;
                case InterpolatedStringExpressionSyntax interp:
                    any = true;
                    foreach (var content in interp.Contents)
                    {
                        if (content is InterpolatedStringTextSyntax t) sb.Append(t.TextToken.ValueText);
                        else sb.Append('{').Append(holes++).Append('}');
                    }
                    break;
                case BinaryExpressionSyntax { RawKind: (int)SyntaxKind.AddExpression } b when IsStringExpression(b, model):
                    Walk(b.Left);
                    Walk(b.Right);
                    break;
                case ParenthesizedExpressionSyntax p when IsStringExpression(p.Expression, model):
                    Walk(p.Expression);
                    break;
                default:
                    sb.Append('{').Append(holes++).Append('}');
                    break;
            }
        }
        Walk(e);
        literal = any;
        return sb.ToString();
    }
}

/// <summary>Which strings are the player's to read, and which are keys, paths, logs and the like.</summary>
static class Rules
{
    /// <summary>Calls whose string arguments are never shown: logs, files, nodes, input, sounds, settings keys, string
    /// handling, lookups by key.</summary>
    static readonly HashSet<string> KeyMethods = new()
    {
        "Print", "PrintErr", "PrintRich", "PushWarning", "PushError", "WriteLine", "Write", "Assert", "Fail",
        "Load", "Exists", "Open", "GetFilesAt", "GetDirectoriesAt", "DirExistsAbsolute", "MakeDirRecursiveAbsolute", "FileExists",
        "ReadAllText", "WriteAllText", "Combine", "GetFileName", "GetExtension", "Replace", "Move", "Copy", "Delete", "SetAside",
        "Read", "Write", "ShellOpen", "GetEnvironment", "HasFeature", "GetSetting", "SetSetting", "GlobalizePath", "LocalizePath",
        "GetNode", "GetNodeOrNull", "HasNode", "FindChild", "Connect", "Disconnect", "EmitSignal", "Call", "CallDeferred",
        "Set", "Get", "SetMeta", "GetMeta", "HasMeta", "AddToGroup", "IsInGroup", "GetNodesInGroup", "CallGroup", "RemoveFromGroup",
        "TweenProperty", "TweenMethod", "AddThemeColorOverride", "AddThemeFontSizeOverride", "AddThemeStyleboxOverride",
        "AddThemeConstantOverride", "AddThemeFontOverride", "AddThemeIconOverride", "GetThemeConstant", "GetThemeColor",
        "GetThemeFont", "GetThemeFontSize", "GetThemeStylebox", "HasThemeConstant", "SetStylebox", "SetColor", "SetConstant",
        "SetFont", "SetFontSize", "SetIcon", "SetShaderParameter", "GetShaderParameter",
        "IsActionPressed", "IsActionJustPressed", "IsActionReleased", "IsActionJustReleased", "IsAction", "ActionPress",
        "ActionRelease", "AddAction", "HasAction", "ActionEraseEvents", "ActionAddEvent",
        "Play", "PlayAt", "Stop", "Sound", "Song", "GetBusIndex", "SetBusName", "AddBus", "Stream", "SetBus", "SetBusSend",
        "SetBusVolumeDb", "SetBusMute", "Invoke", "SetRichPresence",
        "SetValue", "GetValue", "HasSectionKey", "HasSection", "EraseSection", "EraseSectionKey",
        "Split", "Join", "Contains", "StartsWith", "EndsWith", "IndexOf", "LastIndexOf", "Trim", "TrimStart", "TrimEnd",
        "Equals", "Compare", "CompareTo", "Insert", "Remove", "PadLeft", "PadRight", "Parse", "TryParse", "ToString",
        "ContainsKey", "TryGetValue", "GetValueOrDefault", "Arg", "Moment", "Shot", "Has", "Named", "Exists",
        "Serialize", "Deserialize", "Matches", "IsMatch", "Match", "Escape", "Format",
        "GetCultureInfo", "CreateFromFile", "Translate", "TranslatePlural", "SetLocale", "Key",
    };

    /// <summary>Assignments whose strings are keys, paths and names a program uses, never text.</summary>
    static readonly HashSet<string> KeyTargets = new()
    {
        "Key", "Id", "Path", "Icon", "Sprite", "Texture", "Sfx", "Sound", "Song", "Bus", "Group", "Action", "Skin",
        "Font", "Scene", "Shader", "Tag", "Animation", "Stream", "Section", "File", "Folder", "Dir", "Url", "Locale",
        "Names", "FakeName", "Entry", "Renamed", "Image", "Picture", "Portrait", "Figure", "Backdrop", "Setting",
    };

    public static string Context(ExpressionSyntax e, SemanticModel model)
    {
        SyntaxNode child = e;
        var n = e.Parent;
        // Through what passes a value along unchanged: brackets, branches, ?? defaults, arms, arrays, tuples, lambdas.
        while (n is ParenthesizedExpressionSyntax or ConditionalExpressionSyntax or SwitchExpressionArmSyntax or SwitchExpressionSyntax
                   or InitializerExpressionSyntax { RawKind: (int)SyntaxKind.ArrayInitializerExpression or (int)SyntaxKind.CollectionInitializerExpression }
                   or ArrayCreationExpressionSyntax or ImplicitArrayCreationExpressionSyntax or CollectionExpressionSyntax
                   or ExpressionElementSyntax or TupleExpressionSyntax or CastExpressionSyntax or LambdaExpressionSyntax
                   or BinaryExpressionSyntax { RawKind: (int)SyntaxKind.CoalesceExpression }
               || (n is ArgumentSyntax && n.Parent is TupleExpressionSyntax))
        {
            if (n is ConditionalExpressionSyntax c && c.Condition == child) break;
            if (n is SwitchExpressionArmSyntax arm && arm.Expression != child) return "pattern";
            child = n;
            n = n.Parent;
        }
        switch (n)
        {
            case ArgumentSyntax arg when arg.Parent is ArgumentListSyntax { Parent: InvocationExpressionSyntax inv } list:
                return $"{MethodName(inv)}({list.Arguments.IndexOf(arg)})";
            case ArgumentSyntax arg when arg.Parent is ArgumentListSyntax { Parent: BaseObjectCreationExpressionSyntax create } list:
                return $"new {TypeName(create, model)}({list.Arguments.IndexOf(arg)})";
            case ArgumentSyntax arg when arg.Parent is BracketedArgumentListSyntax:
                return "index";
            case AssignmentExpressionSyntax assign when assign.Right == child:
                return assign.Left switch
                {
                    ImplicitElementAccessSyntax => $"{Owner(assign)} =",
                    IdentifierNameSyntax id => $"{id.Identifier.Text} {assign.OperatorToken.Text}",
                    MemberAccessExpressionSyntax m => $"{m.Name.Identifier.Text} {assign.OperatorToken.Text}",
                    _ => $"assign {assign.OperatorToken.Text}",
                };
            case EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax v }:
                bool isConst = v.Parent?.Parent is FieldDeclarationSyntax f && f.Modifiers.Any(SyntaxKind.ConstKeyword)
                               || v.Parent?.Parent is LocalDeclarationStatementSyntax l && l.IsConst;
                return isConst ? "const" : $"var {v.Identifier.Text}";
            case EqualsValueClauseSyntax { Parent: PropertyDeclarationSyntax p }:
                return $"{p.Identifier.Text} =";
            case EqualsValueClauseSyntax { Parent: ParameterSyntax }:
                return "default";
            case ArrowExpressionClauseSyntax { Parent: PropertyDeclarationSyntax p }:
                return $"{p.Identifier.Text} =>";
            case ArrowExpressionClauseSyntax { Parent: MethodDeclarationSyntax m }:
                return $"return {m.Identifier.Text}";
            case ArrowExpressionClauseSyntax { Parent: LocalFunctionStatementSyntax lf }:
                return $"return {lf.Identifier.Text}";
            case ReturnStatementSyntax ret:
                var method = ret.Ancestors().FirstOrDefault(a => a is MethodDeclarationSyntax or LocalFunctionStatementSyntax or AccessorDeclarationSyntax or LambdaExpressionSyntax);
                return method switch
                {
                    MethodDeclarationSyntax m => $"return {m.Identifier.Text}",
                    LocalFunctionStatementSyntax lf => $"return {lf.Identifier.Text}",
                    AccessorDeclarationSyntax { Parent.Parent: PropertyDeclarationSyntax p } => $"return {p.Identifier.Text}",
                    _ => "return",
                };
            case BinaryExpressionSyntax b when b.IsKind(SyntaxKind.EqualsExpression) || b.IsKind(SyntaxKind.NotEqualsExpression):
                return "compare";
            case ConstantPatternSyntax or CaseSwitchLabelSyntax or IsPatternExpressionSyntax:
                return "pattern";
            case InterpolationSyntax:
                return "hole";
            case AttributeArgumentSyntax:
                return "attribute";
            case ImplicitElementAccessSyntax or BracketedArgumentListSyntax:
                return "index";
            case YieldStatementSyntax:
                return "yield";
            case ThrowExpressionSyntax or ThrowStatementSyntax:
                return "throw";
            default:
                return n?.Kind().ToString() ?? "?";
        }
    }

    /// <summary>The field or property a dictionary initializer fills ("Renamed" for Renamed = new() { ["a"] = "b" }).</summary>
    static string Owner(SyntaxNode node)
    {
        foreach (var a in node.Ancestors())
        {
            if (a is VariableDeclaratorSyntax v) return v.Identifier.Text;
            if (a is PropertyDeclarationSyntax p) return p.Identifier.Text;
            if (a is AssignmentExpressionSyntax { Left: IdentifierNameSyntax id }) return id.Identifier.Text;
            if (a is MemberDeclarationSyntax or StatementSyntax) break;
        }
        return "dict";
    }

    static string MethodName(InvocationExpressionSyntax inv) => inv.Expression switch
    {
        MemberAccessExpressionSyntax m => m.Name.Identifier.Text,
        IdentifierNameSyntax id => id.Identifier.Text,
        GenericNameSyntax g => g.Identifier.Text,
        MemberBindingExpressionSyntax mb => mb.Name.Identifier.Text,
        _ => inv.Expression.ToString(),
    };

    static string TypeName(BaseObjectCreationExpressionSyntax create, SemanticModel model)
    {
        if (create is ObjectCreationExpressionSyntax o) return o.Type.ToString();
        return model.GetTypeInfo(create).Type?.Name ?? "?";
    }

    /// <summary>Text a player reads: has letters, and isn't shaped like a key, a path, a colour, a flag or a format.</summary>
    public static bool LooksLikeText(string s)
    {
        string bare = Regex.Replace(s, @"\{\d+\}", "");
        if (!Regex.IsMatch(bare, "[A-Za-z]")) return false;
        if (Regex.IsMatch(bare, @"^[a-z0-9_]+$")) return false;
        if (Regex.IsMatch(bare, @"^#[0-9a-fA-F]{3,8}$")) return false;
        if (bare.Contains("://") || bare.StartsWith("res:") || bare.StartsWith("user:")) return false;
        if (Regex.IsMatch(s, @"^[\w./{}-]*\.(png|jpg|webp|json|wav|ogg|mp3|tscn|tres|cfg|po|pot|txt|ttf|import|uid|bak|tmp|exe|zip)$", RegexOptions.IgnoreCase)) return false;
        if (!s.Contains(' ') && s.Contains('/')) return false;
        if (Regex.IsMatch(bare, @"^[a-z_]+(:[a-z_]+)+$")) return false;
        // A tale's beat or flag ("magpie.notice", "pip.apprentice"), or a prefix for one ("away:", "offer:").
        if (Regex.IsMatch(bare, @"^[a-z_]+(\.[a-z_]+)+$") || Regex.IsMatch(bare, @"^[a-z_]+:$")) return false;
        if (Regex.IsMatch(bare, @"^-{1,2}[a-z]")) return false;
        if (Regex.IsMatch(bare, @"^[NnFfPpCcDdEeXxGgRr]\d*$")) return false;
        if (Regex.IsMatch(bare, @"^[a-z][a-zA-Z0-9]*$") && bare.Any(char.IsUpper)) return false;
        return true;
    }

    /// <summary>Arguments by position: a thing's and a Heartsong's names are keys kept in saves; their sound words are
    /// the slip's ("jingle"), their sound and sprite names the program's.</summary>
    static readonly Dictionary<string, string> Positions = new()
    {
        ["new CategoryDef(2)"] = "wrap", ["new CategoryDef(3)"] = "skip: sound", ["new CategoryDef(12)"] = "key",
        ["new CategoryDef(13)"] = "skip: sprite", ["new CategoryDef(14)"] = "key", ["new CategoryDef(15)"] = "skip: sprite",
        ["new HeartsongDef(1)"] = "key", ["new HeartsongDef(2)"] = "wrap", ["new HeartsongDef(6)"] = "skip: sprite",
        ["Replace(1)"] = "wrap", ["new AchievementDef(0)"] = "skip: key",
    };

    /// <summary>Where a lowercase word ("jingle", "gently", "tonight") is something the player reads, not a key.</summary>
    static readonly HashSet<string> DisplayWords = new()
    {
        "return Number", "return Shape", "return HeftWord", "return Adverb", "return LevelName", "MakeLabel(0)", "Note =>",
        "var price", "Better =>", "return One", "var list", "var when", "MakeGutMeter(2)", "MakeGutMeter(3)", "return Trade",
        "return TagName", "new HeartsongDef(2)", "new CategoryDef(2)", "Replace(1)",
    };

    /// <summary>What to do with a string: "wrap", "key" (a name kept English, marked), or why it's left alone.</summary>
    public static string Verdict(Found f)
    {
        string c = f.Context;
        if (Positions.TryGetValue(c, out string fixedVerdict) && fixedVerdict.StartsWith("skip")) return fixedVerdict;
        bool word = Regex.IsMatch(f.Text, @"^[a-z]{2,}( [a-z]+)*$");
        if (word && DisplayWords.Contains(c)) return Positions.GetValueOrDefault(c, "wrap");
        if (!LooksLikeText(f.Text)) return "skip: not text";
        if (fixedVerdict != null) return fixedVerdict;
        if (c is "const" or "pattern" or "attribute" or "default" or "index" or "compare" or "throw") return $"skip: {c}";
        if (c.EndsWith(')'))
        {
            string method = c[..c.IndexOf('(')];
            if (method.StartsWith("new ") && method.EndsWith("Exception")) return "skip: exception";
            if (KeyMethods.Contains(method)) return $"skip: {method}()";
        }
        string target = c.Split(' ')[0];
        if (c.EndsWith(" =") || c.EndsWith(" =>") || c.EndsWith(" +="))
        {
            if (KeyTargets.Contains(target)) return target is "Names" or "FakeName" ? "key" : $"skip: {target}";
        }
        return "wrap";
    }
}

static class Survey
{
    public static int Run(Game game, bool verbose)
    {
        var all = new List<Found>();
        foreach (var tree in game.Trees.Where(game.Translated))
            foreach (var f in Strings.In(game, tree))
            {
                f.Verdict = Rules.Verdict(f);
                all.Add(f);
            }
        Console.WriteLine($"{all.Count} strings: {all.Count(f => f.Verdict == "wrap")} to wrap, {all.Count(f => f.Verdict == "key")} names kept as keys, " +
                          $"{all.Count(f => f.Verdict.StartsWith("skip"))} left alone");
        foreach (var group in all.GroupBy(f => (f.Verdict, f.Context)).OrderBy(g => g.Key.Verdict).ThenByDescending(g => g.Count()))
        {
            if (!verbose && group.Key.Verdict == "skip: not text") continue;
            Console.WriteLine($"\n[{group.Key.Verdict}] {group.Key.Context}  x{group.Count()}");
            foreach (var f in group.Take(verbose ? 1000 : 4))
                Console.WriteLine($"    {game.Rel(f.Tree.FilePath)}:{f.Line}  {Short(f.Text)}");
        }
        return 0;
    }

    /// <summary>Texts still unwrapped that the rules would wrap: a new one added without Loc.T.</summary>
    public static int Check(Game game)
    {
        int count = 0;
        foreach (var tree in game.Trees.Where(game.Translated))
            foreach (var f in Strings.In(game, tree).Where(f => Rules.Verdict(f) is "wrap" or "key" && !Marked(f)))
            {
                Console.WriteLine($"{game.Rel(f.Tree.FilePath)}:{f.Line}  [{f.Context}]  {Short(f.Text)}");
                count++;
            }
        // An enum in a hole shows its English name ("Coins"): name it through a function that says it in words.
        int enums = 0;
        foreach (var tree in game.Trees.Where(game.Translated))
        {
            var model = game.Compilation.GetSemanticModel(tree);
            foreach (var hole in tree.GetRoot().DescendantNodes().OfType<InterpolationSyntax>())
            {
                if (model.GetTypeInfo(hole.Expression).Type is not { TypeKind: TypeKind.Enum } type) continue;
                if (!hole.Ancestors().OfType<InvocationExpressionSyntax>().Any(Strings.IsLoc)) continue;
                int line = tree.GetLineSpan(hole.Span).StartLinePosition.Line + 1;
                if (tree.GetText().Lines[line - 1].ToString().Contains("// loc: keys")) continue;
                Console.WriteLine($"{game.Rel(tree.FilePath)}:{line}  an enum ({type.Name}) shown by its name: {hole}");
                enums++;
            }
        }
        if (enums > 0) Console.WriteLine($"{enums} enums shown by their English names.");
        Console.WriteLine(count == 0 ? "Every text is wrapped." : $"{count} texts aren't wrapped in Loc.T (or marked as left alone).");
        return count + enums == 0 ? 0 : 1;
    }

    /// <summary>A line that says its strings are keys on purpose ("// loc: keys").</summary>
    static bool Marked(Found f) => f.Tree.GetText().Lines[f.Line - 1].ToString().Contains("// loc: keys");

    public static string Short(string s)
    {
        s = s.Replace("\n", "\\n");
        return s.Length > 100 ? s[..97] + "..." : s;
    }
}

/// <summary>Wraps each text in Loc.T (or Loc.Key), turning a + chain into one interpolated string so it's one sentence.</summary>
static class Wrapper
{
    public static int Run(Game game, HashSet<string> only)
    {
        int total = 0;
        foreach (var tree in game.Trees.Where(game.Translated))
        {
            if (only.Count > 0 && !only.Contains(Path.GetFullPath(tree.FilePath))) continue;
            var found = Strings.In(game, tree).Where(f => (f.Verdict = Rules.Verdict(f)) is "wrap" or "key").ToList();
            if (found.Count == 0) continue;
            var model = game.Compilation.GetSemanticModel(tree);
            var targets = found.ToDictionary(f => f.Node, f => f.Verdict);
            var root = tree.GetRoot().ReplaceNodes(targets.Keys, (original, rewritten) => Wrap(rewritten, original, model, targets[original]));
            if (root is CompilationUnitSyntax unit && NeedsUsing(unit, tree))
                unit = unit.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(" Keywright.Core")).WithTrailingTrivia(SyntaxFactory.ElasticLineFeed));
            else unit = (CompilationUnitSyntax)root;
            File.WriteAllText(tree.FilePath, unit.ToFullString(), new UTF8Encoding(false));
            Console.WriteLine($"{game.Rel(tree.FilePath)}: {found.Count}");
            total += found.Count;
        }
        Console.WriteLine($"{total} texts wrapped.");
        return 0;
    }

    static bool NeedsUsing(CompilationUnitSyntax unit, SyntaxTree tree)
    {
        string text = unit.ToFullString();
        if (Regex.IsMatch(text, @"^\s*namespace\s+Keywright\.Core\b", RegexOptions.Multiline)) return false;
        return !unit.Usings.Any(u => u.Name?.ToString() == "Keywright.Core");
    }

    static ExpressionSyntax Wrap(SyntaxNode rewritten, ExpressionSyntax original, SemanticModel model, string verdict)
    {
        var expr = (ExpressionSyntax)rewritten;
        // A + chain with something that isn't text in it becomes one interpolated string: one sentence to translate.
        if (expr is BinaryExpressionSyntax && Strings.Template(original, model, out _).Contains("{0}"))
            expr = ToInterpolated(expr, model, original);
        string method = verdict == "key" ? "Key" : "T";
        var call = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("Loc"), SyntaxFactory.IdentifierName(method)),
                SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(expr.WithoutTrivia()))))
            .WithLeadingTrivia(expr.GetLeadingTrivia()).WithTrailingTrivia(expr.GetTrailingTrivia());
        return call;
    }

    /// <summary>"a " + x + " b" as $"a {x} b" (braces in the literals doubled).</summary>
    static ExpressionSyntax ToInterpolated(ExpressionSyntax chain, SemanticModel model, ExpressionSyntax original)
    {
        var sb = new StringBuilder("$\"");
        void Walk(ExpressionSyntax x)
        {
            switch (x)
            {
                case LiteralExpressionSyntax { RawKind: (int)SyntaxKind.StringLiteralExpression } lit:
                    sb.Append(Escape(lit.Token.ValueText));
                    break;
                case InterpolatedStringExpressionSyntax interp when interp.StringStartToken.Text == "$\"":
                    foreach (var content in interp.Contents) sb.Append(content.ToString());
                    break;
                case BinaryExpressionSyntax { RawKind: (int)SyntaxKind.AddExpression } b:
                    Walk(b.Left);
                    Walk(b.Right);
                    break;
                case ParenthesizedExpressionSyntax p when p.Expression is BinaryExpressionSyntax { RawKind: (int)SyntaxKind.AddExpression } or LiteralExpressionSyntax:
                    Walk(p.Expression);
                    break;
                default:
                    string hole = x.WithoutTrivia().ToString();
                    // A conditional or a lambda in a hole needs brackets.
                    if (x is ConditionalExpressionSyntax || hole.Contains(':')) hole = $"({hole})";
                    sb.Append('{').Append(hole).Append('}');
                    break;
            }
        }
        // Walk the rewritten chain (inner texts may have been wrapped already).
        Walk(chain);
        sb.Append('"');
        return SyntaxFactory.ParseExpression(sb.ToString());
    }

    static string Escape(string s)
    {
        var sb = new StringBuilder();
        foreach (char c in s)
        {
            sb.Append(c switch
            {
                '{' => "{{", '}' => "}}", '"' => "\\\"", '\\' => "\\\\", '\n' => "\\n", '\r' => "\\r", '\t' => "\\t",
                _ => c.ToString(),
            });
        }
        return sb.ToString();
    }
}

/// <summary>Writes locale/keywright.pot: every msgid, with where it's used and a comment for the translator.</summary>
static class Extractor
{
    public static int Run(Game game, string root)
    {
        var entries = new Dictionary<(string Id, string Plural), (List<string> Refs, List<string> Notes)>();
        var order = new List<(string, string)>();
        foreach (var tree in game.Trees.Where(game.Translated))
        {
            var model = game.Compilation.GetSemanticModel(tree);
            foreach (var inv in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().Where(Strings.IsLoc))
            {
                string name = ((MemberAccessExpressionSyntax)inv.Expression).Name.Identifier.Text;
                var args = inv.ArgumentList.Arguments;
                (string, string) key;
                if (name is "T" or "Key" && args.Count == 1)
                {
                    string id = Strings.Template(args[0].Expression, model, out bool literal);
                    if (!literal || !Rules.LooksLikeText(id)) continue;
                    key = (id, null);
                }
                else if (name == "Plural" && args.Count == 3)
                {
                    string one = Strings.Template(args[1].Expression, model, out bool l1), many = Strings.Template(args[2].Expression, model, out bool l2);
                    if (!l1 || !l2) continue;
                    key = (one, many);
                }
                else continue;
                if (!entries.TryGetValue(key, out var entry))
                {
                    entries[key] = entry = (new List<string>(), new List<string>());
                    order.Add(key);
                }
                int line = tree.GetLineSpan(inv.Span).StartLinePosition.Line + 1;
                entry.Refs.Add($"{game.Rel(tree.FilePath)}:{line}");
                string note = Note(inv, tree);
                if (note != null && !entry.Notes.Contains(note)) entry.Notes.Add(note);
            }
        }
        var sb = new StringBuilder();
        sb.Append("# Keywright: every text the player reads. Made by tools/loc (dotnet run --project tools/loc -- extract);\n");
        sb.Append("# don't edit it: translate it into a <locale>.po beside it (de.po, pt_BR.po, zh_CN.po), e.g. with Poedit.\n");
        sb.Append("# {0}, {1}... are filled in by the game (numbers, names, other texts): keep them, in any order.\n");
        sb.Append("# {price}, {act} and the like are filled in too: keep them as they are.\n");
        sb.Append("msgid \"\"\nmsgstr \"\"\n\"Project-Id-Version: Keywright\\n\"\n\"MIME-Version: 1.0\\n\"\n");
        sb.Append("\"Content-Type: text/plain; charset=UTF-8\\n\"\n\"Content-Transfer-Encoding: 8bit\\n\"\n\"Plural-Forms: nplurals=2; plural=(n != 1);\\n\"\n");
        foreach (var key in order)
        {
            var (refs, notes) = entries[key];
            sb.Append('\n');
            foreach (string note in notes) sb.Append("#. ").Append(note).Append('\n');
            foreach (string chunk in Chunk(refs, 100)) sb.Append("#: ").Append(chunk).Append('\n');
            if (key.Item1.Contains("{0}") || key.Item1.Contains("{1}")) sb.Append("#, csharp-format\n");
            sb.Append("msgid ").Append(Quote(key.Item1)).Append('\n');
            if (key.Item2 != null)
            {
                sb.Append("msgid_plural ").Append(Quote(key.Item2)).Append('\n');
                sb.Append("msgstr[0] \"\"\nmsgstr[1] \"\"\n");
            }
            else sb.Append("msgstr \"\"\n");
        }
        string dir = Path.Combine(root, "locale");
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, "keywright.pot");
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
        Console.WriteLine($"{order.Count} texts ({order.Sum(k => k.Item1.Length + (k.Item2?.Length ?? 0))} characters) in {Path.GetRelativePath(root, path)}");
        return 0;
    }

    /// <summary>A "// tr: ..." comment on the line above or at the end of the line, for the translator.</summary>
    static string Note(SyntaxNode node, SyntaxTree tree)
    {
        var line = tree.GetText().Lines[tree.GetLineSpan(node.Span).StartLinePosition.Line];
        string text = line.ToString();
        int at = text.IndexOf("// tr:", StringComparison.Ordinal);
        if (at >= 0) return text[(at + 6)..].Trim();
        int number = tree.GetLineSpan(node.Span).StartLinePosition.Line;
        if (number > 0)
        {
            string above = tree.GetText().Lines[number - 1].ToString().Trim();
            if (above.StartsWith("// tr:")) return above[6..].Trim();
        }
        return null;
    }

    static IEnumerable<string> Chunk(List<string> refs, int width)
    {
        var line = new StringBuilder();
        foreach (string r in refs)
        {
            if (line.Length > 0 && line.Length + r.Length + 1 > width)
            {
                yield return line.ToString();
                line.Clear();
            }
            if (line.Length > 0) line.Append(' ');
            line.Append(r);
        }
        if (line.Length > 0) yield return line.ToString();
    }

    static string Quote(string s)
    {
        var sb = new StringBuilder("\"");
        foreach (char c in s)
        {
            sb.Append(c switch { '"' => "\\\"", '\\' => "\\\\", '\n' => "\\n", '\t' => "\\t", '\r' => "\\r", _ => c.ToString() });
        }
        return sb.Append('"').ToString();
    }
}
