shader_type canvas_item;

// The Gilding (scripts/Ui/Gilt.cs). For Bram, the mask's red channel says at which step a pixel turns to gold (a
// quarter: the hand; a half: the forearm; three quarters: the throat; nothing: never), and the hand is gloved while
// he hides it. A statue (whole) is gold all over. Shading is kept: gold is a ramp of the pixel's own brightness, lit
// from the upper left like the art, with a slow glint passing over it.
uniform sampler2D mask : filter_nearest, hint_default_black;
uniform float stage = 0.0;
uniform bool glove = false;
uniform bool whole = false;

vec3 ramp(float l, vec3 dark, vec3 mid, vec3 light) {
	return l < 0.5 ? mix(dark, mid, l * 2.0) : mix(mid, light, (l - 0.5) * 2.0);
}

void fragment() {
	vec4 c = texture(TEXTURE, UV);
	float at = whole ? 1.0 : floor(texture(mask, UV).r * 4.0 + 0.5);
	if (c.a > 0.0 && at > 0.5) {
		float l = clamp(dot(c.rgb, vec3(0.3, 0.55, 0.15)), 0.0, 1.0);
		if (!whole && at < 1.5 && glove) {
			// Plain brown leather over the hand.
			c.rgb = ramp(l, vec3(0.16, 0.10, 0.07), vec3(0.34, 0.22, 0.14), vec3(0.55, 0.38, 0.24));
		} else if (whole || at <= stage + 0.01) {
			vec3 g = ramp(l, vec3(0.40, 0.22, 0.05), vec3(0.86, 0.60, 0.16), vec3(1.0, 0.93, 0.58));
			float sweep = fract(TIME * 0.15) * 1.8 - 0.4;
			float d = abs((UV.x + UV.y) * 0.5 - sweep);
			g += vec3(0.32, 0.27, 0.12) * smoothstep(0.04, 0.0, d);
			c.rgb = g;
		}
	}
	COLOR = c;
}
