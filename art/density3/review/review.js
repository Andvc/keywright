'use strict';
const D=window.REVIEW_DATA;
const $=id=>document.getElementById(id);
const esc=s=>String(s??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
let tab='figures', visible=[], current=null, currentIndex=0, zoom='fit', review={};
const storageKey='keywright-density3-review-v1';
try{review=JSON.parse(localStorage.getItem(storageKey)||'{}')}catch{}
const identity=a=>(a.kind||'asset')+':'+a.id;
const getReview=a=>{let r=review[identity(a)];const approved=a.user_approval==='approved';if(r?.sha256===a.sha256&&(!approved||r.updated>D.approval_recorded_at))return r;return {status:approved?'pass':'pending',note:''}};
function save(){try{localStorage.setItem(storageKey,JSON.stringify(review))}catch{}updateProgress()}
function setReview(status){if(!current||current.kind==='contact')return;review[identity(current)]={id:current.id,kind:current.kind||'asset',sha256:current.sha256,status,note:$('note').value,updated:new Date().toISOString()};save();$('position').textContent=`${currentIndex+1} / ${visible.length} · ${status==='pass'?'通过':status==='fix'?'需修改':'未审'}`;renderGrid(false)}
const screenLabels={figures:'人物实拍',shops:'店铺与时段',stories:'故事场景',notebook:'笔记与头像',locks:'开锁界面',upgrades:'升级界面'};
const screens=D.screens.map(s=>({...s,kind:'screen',title:s.title_zh||s.id,url:s.file,size:[s.width,s.height],category_zh:screenLabels[s.category]||'游戏实拍'}));
const contacts=D.contacts.map(s=>({...s,kind:'contact',url:s.file,thumb:s.file,category:s.id.split('_')[0],category_zh:s.title}));
function universe(){return tab==='figures'?D.assets.filter(a=>a.category==='figure'):tab==='screens'?screens:tab==='contacts'?contacts:D.assets}
function updateProgress(){const all=[...D.assets,...screens];let pass=0,fix=0;for(const a of all){const r=getReview(a);pass+=r.status==='pass';fix+=r.status==='fix'}$('progress').textContent=`已审 ${pass+fix} / ${all.length} · 需修改 ${fix}`}
function filters(){const previous=$('category').value;const categories=new Map(universe().map(a=>[a.category,a.category_zh||a.category]));$('category').innerHTML='<option value="">全部类别</option>'+[...categories].map(([k,v])=>`<option value="${esc(k)}">${esc(v)}</option>`).join('');$('category').value=categories.has(previous)?previous:''}
function renderGrid(rebuild=true){
 if(rebuild){const q=$('search').value.trim().toLowerCase(),category=$('category').value,status=$('status').value;
 visible=universe().filter(a=>(!category||a.category===category)&&(!q||`${a.id} ${a.title} ${a.category_zh}`.toLowerCase().includes(q))&&(!status||getReview(a).status===status));}
 $('grid').innerHTML=visible.map((a,i)=>{const r=getReview(a);return `<button class="card" data-index="${i}"><span class="thumb"><img loading="lazy" src="${esc(a.thumb)}" alt="${esc(a.title)}"></span><strong>${esc(a.title)}</strong><small>${esc(a.id)}</small><span class="tags"><span class="tag ${r.status}">${r.status==='pass'?'通过':r.status==='fix'?'需修改':'未审'}</span>${a.retained?'<span class="tag">沿用原版</span>':''}${a.face_edit?'<span class="tag special">表情修正</span>':''}${a.face_protection?'<span class="tag special">五官保留</span>':''}${a.face?.qa==='watch'?'<span class="tag fix">重点复查</span>':''}</span></button>`}).join('');
 $('notice').textContent=tab==='figures'?'38 位人物均经 subagent 分组检查。点开可看全身、原图与最终脸部、游戏头像；这些检查不代表你的最终批准。':tab==='screens'?`这里是游戏实际渲染的 ${D.screens.length} 张 2560 × 1440 截图，覆盖人物、四家店的四个时段、故事、笔记、锁和升级界面。`:tab==='contacts'?'分类总览便于检查整体风格；逐张确认请用「全部资产」。总览经过缩放，不用于判断实际颗粒度。':`完整清单共 ${D.counts.assets} 项：${D.counts.processed} 项制作 3px 版；${D.counts.retained} 项界面、特效、品牌及宣传图沿用原版，并已列明。`;
 updateProgress();
}
function applyZoom(){
 const img=$('mainimage');if(!img.naturalWidth)return;
 const native=img.naturalWidth/(window.devicePixelRatio||1);
 const effective=document.fullscreenElement?'1':zoom;
 $('stage').classList.toggle('fit',effective==='fit');
 img.style.width=effective==='fit'?'auto':(native*Number(effective))+'px';img.style.height='auto';
 $('zoomlabel').textContent=effective==='fit'?'适应窗口：有缩放':`${Number(effective)*100}% · 以屏幕物理像素计`;
}
function facePanel(a){
 $('faces').hidden=!a.face;$('facegrid').innerHTML='';if(!a.face)return;
 const f=a.face,entries=[['原始画面 · 未像素化',f.source,f.size],['表情修正原图',f.corrected,f.size],['修正前的 3px',f.first,f.size],['最终 3px',f.final,f.size],['游戏人物头像',f.portrait,null],['游戏队列头像',f.queue,null]].filter(x=>x[1]);
 for(const [label,path,size]of entries){const box=document.createElement('div');box.className='facecard';box.innerHTML=`<strong>${esc(label)}</strong><img src="${esc(path)}" alt="${esc(label)}"><button data-facezoom="1">实际大小</button><button data-facezoom="2">放大 2 倍</button><button data-facezoom="4">放大 4 倍</button>`;const im=box.querySelector('img');const resize=factor=>{im.style.width=(im.naturalWidth*factor/(window.devicePixelRatio||1))+'px';im.style.height='auto'};im.onload=()=>resize(2);box.querySelectorAll('button').forEach(b=>b.onclick=()=>resize(+b.dataset.facezoom));$('facegrid').append(box)}
}
function openAt(index){
 if(!visible.length)return;currentIndex=(index+visible.length)%visible.length;current=visible[currentIndex];const a=current,r=getReview(a);
 $('detailid').textContent=a.id;$('detailtitle').textContent=a.title;$('detailmeta').textContent=`${a.category_zh||a.category||''} · ${a.size?a.size.join(' × ')+' px':''}${a.retained?' · 沿用原版':a.kind==='screen'?' · 游戏实拍':a.kind==='contact'?' · 分类缩略总览':' · 3px 版本'}`;
 $('detailnote').textContent=a.face?(a.face.note||'请确认五官完整、表情自然。'):(a.note_zh||(a.retained?'此项为沿用资产，也列入完整审核范围。':''));
 $('mainimage').src=a.url;$('mainimage').alt=a.title;$('original').href=a.url;$('note').value=r.note||'';
 $('position').textContent=`${currentIndex+1} / ${visible.length} · ${r.status==='pass'?'通过':r.status==='fix'?'需修改':'未审'}`;
 $('inspector').querySelector('.reviewbox').hidden=a.kind==='contact';facePanel(a);zoom='fit';applyZoom();
 if(!$('inspector').open)$('inspector').showModal();$('inspector').scrollTop=0;
}
$('grid').addEventListener('click',e=>{const b=e.target.closest('[data-index]');if(b)openAt(+b.dataset.index)});
$('tabs').addEventListener('click',e=>{const b=e.target.closest('[data-tab]');if(!b)return;tab=b.dataset.tab;document.querySelectorAll('[data-tab]').forEach(x=>x.classList.toggle('active',x===b));filters();renderGrid()});
for(const id of ['category','status','search'])$(id).addEventListener(id==='search'?'input':'change',()=>renderGrid());
$('close').onclick=()=>$('inspector').close();$('prev').onclick=()=>openAt(currentIndex-1);$('next').onclick=()=>openAt(currentIndex+1);
$('markfix').onclick=()=>setReview('fix');$('markpass').onclick=()=>setReview('pass');$('passnext').onclick=()=>{setReview('pass');openAt(currentIndex+1)};
$('note').oninput=()=>setReview(getReview(current).status);
document.querySelectorAll('[data-zoom]').forEach(b=>b.onclick=()=>{zoom=b.dataset.zoom;applyZoom()});
$('mainimage').onload=applyZoom;
async function fullscreen(){if(document.fullscreenElement)await document.exitFullscreen();else await $('stage').requestFullscreen()}
$('fullscreen').onclick=fullscreen;document.addEventListener('fullscreenchange',applyZoom);window.addEventListener('resize',applyZoom);
document.addEventListener('keydown',e=>{if(!$('inspector').open||/INPUT|TEXTAREA|SELECT/.test(e.target.tagName))return;if(e.key==='ArrowRight')openAt(currentIndex+1);if(e.key==='ArrowLeft')openAt(currentIndex-1);if(e.key.toLowerCase()==='f')fullscreen();if(e.key==='1'){zoom='1';applyZoom()}});
$('export').onclick=()=>{const all=[...D.assets,...screens];const payload={project:'Keywright',version:'3px at 2560x1440',exported:new Date().toISOString(),upload_authorized:D.user_approval==='approved',items:all.map(a=>({id:a.id,kind:a.kind||'asset',sha256:a.sha256,...getReview(a)}))};$('exporttext').value=JSON.stringify(payload,null,2);$('exportdialog').showModal()};
$('exportclose').onclick=()=>$('exportdialog').close();
$('copyreport').onclick=async()=>{try{await navigator.clipboard.writeText($('exporttext').value);$('copystatus').textContent='已复制完整记录。'}catch{$('exporttext').focus();$('exporttext').select();$('copystatus').textContent='记录已选中，请按 Ctrl+C 复制。'}};
$('summary').textContent=`${D.counts.figures} 位人物 · ${D.counts.assets} 项资产 · ${D.counts.screens} 张游戏实拍`;
if(D.user_approval==='approved')document.querySelector('.pending').textContent='用户已全部审核通过 · 已授权上传';
filters();renderGrid();
