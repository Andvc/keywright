"""Build the offline, complete user-review packet. Never uploads or approves art."""
import argparse
import hashlib
import json
import math
import shutil
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / 'art/density3'
REVIEW = ART / 'review'
WORK = ROOT.parent / 'records/density3_review_20261104'
LABELS = {'figure':'人物', 'sky':'天空', 'place':'场地', 'frame':'店面框架', 'shelf':'货架',
          'counter':'桌台', 'sheet':'报价纸', 'prop':'店铺道具', 'scene':'故事场景',
          'item':'商品', 'heartsong':'剧情商品', 'curio':'珍品', 'icon_upgrade':'升级图标',
          'icon_kind':'分类图标', 'emblem':'徽章', 'story':'剧情钥匙', 'lock_face':'锁面',
          'ui':'界面（沿用）', 'fx':'特效（沿用）', 'brand':'品牌（沿用）', 'store':'商店宣传（沿用）'}


def read(path, default=None):
    return json.loads(path.read_text(encoding='utf-8')) if path.is_file() else default


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def face_info():
    info = {}
    first = read(WORK / 'agent_reviews/a/final_face_index.json', {})
    for name, row in first.items():
        info['figure.'+name] = {'source_box':row['source_face_box'], 'box':row['physical_face_box']}
    for row in read(WORK / 'agent_reviews/b/final_3px_review.json', {}).get('items', []):
        info[row['id']] = {'source_box':row['face_bbox_source_pixels'], 'box':row['face_bbox_output_pixels']}
    for team in ('a','b'):
        # Final reports replace earlier observations, but keep first-pass reports as provenance.
        for filename in ('final_3px_review.json', 'final_verified_review.json'):
            report = read(WORK / f'agent_reviews/{team}/{filename}', {})
            for row in report.get('items', []):
                key = row['id'] if row['id'].startswith('figure.') else 'figure.'+row['id']
                if key not in info:
                    continue
                info[key].update(note=row.get('observation_zh', row.get('review_note_zh', row.get('note_zh',''))),
                                 qa=row.get('qa_verdict', row.get('review_verdict', row.get('verdict','pending'))),
                                 reviewer='face_audit_'+team,
                                 reviewed_sha256=row.get('output_sha256',row.get('production_sha256_at_visual_review','')))
    return info


def crop_faces(row, detail):
    key = row['id'].replace('.', '_')
    box = tuple(detail['box'])
    final = Image.open(ROOT / row['file']).convert('RGBA')
    path = REVIEW / f'faces/{key}_final.png'
    final.crop(box).save(path)
    detail['final'] = path.relative_to(REVIEW).as_posix()
    detail['size'] = [box[2]-box[0], box[3]-box[1]]
    source = ROOT.parent / 'records' / row['original_source']
    transform = row['source_transform']
    sx, sy, tx, ty = [transform[k] for k in ('scale_x','scale_y','translate_x','translate_y')]
    for label, src in [('source', source), ('corrected', WORK / 'source_overrides' / (key+'.png'))]:
        if not src.is_file():
            continue
        original = Image.open(src).convert('RGBA')
        aligned = original.transform(final.size, Image.Transform.AFFINE,
                                     (1/sx,0,-tx/sx,0,1/sy,-ty/sy),Image.Resampling.NEAREST)
        path = REVIEW / f'faces/{key}_{label}.png'
        aligned.crop(box).save(path)
        detail[label] = path.relative_to(REVIEW).as_posix()
    first = WORK / 'first_pass/figures' / (row['id'].split('.')[1]+'.png')
    if first.is_file() and digest(first) != row['output_sha256']:
        path = REVIEW / f'faces/{key}_first.png'
        Image.open(first).crop(box).save(path)
        detail['first'] = path.relative_to(REVIEW).as_posix()
    name = row['id'].split('.')[1]
    detail['portrait'] = '../../../../assets/sprites/sellers/'+name+'.png'
    detail['queue'] = '../../../../assets/sprites/sellers/faces/'+name+'.png'
    return detail


def thumbnail(source, output, bounds=(256,180)):
    im = Image.open(source).convert('RGBA')
    im.thumbnail(bounds, Image.Resampling.NEAREST)
    im.save(output)


def contacts(assets):
    font_path = Path('C:/Windows/Fonts/msyh.ttc')
    font = ImageFont.truetype(str(font_path), 17) if font_path.is_file() else ImageFont.load_default()
    records=[]
    for category in LABELS:
        if category == 'figure':
            continue
        rows = [a for a in assets if a['category'] == category]
        limit = 12 if category in {'sky','place','frame','shelf','counter','sheet','scene','store','brand'} else 24
        for part in range(math.ceil(len(rows)/limit)):
            group=rows[part*limit:(part+1)*limit]
            columns = 3 if limit == 12 else 4
            cw,ch=400,290
            page=Image.new('RGB',(cw*columns,ch*math.ceil(len(group)/columns)+52),'#171d25')
            draw=ImageDraw.Draw(page)
            draw.text((18,12),f'{LABELS[category]} · {part+1} / {math.ceil(len(rows)/limit)}',font=font,fill='#fff3dc')
            for i,a in enumerate(group):
                x,y=(i%columns)*cw,(i//columns)*ch+52
                im=Image.open(ROOT/a['file']).convert('RGBA')
                scale=min((cw-24)/im.width,(ch-62)/im.height)
                # Enlarge small icons only by whole factors for readable contacts.
                scale=max(1,int(scale)) if scale>=1 else scale
                im=im.resize((max(1,round(im.width*scale)),max(1,round(im.height*scale))),Image.Resampling.NEAREST)
                page.paste(im,(x+(cw-im.width)//2,y+8),im)
                draw.text((x+12,y+ch-46),a['id'],font=font,fill='#d8deeb')
                draw.text((x+12,y+ch-24),'沿用' if a['retained'] else '3px',font=font,fill='#aab8b1')
            path=REVIEW/f'contacts/{category}_{part+1}.png'
            page.save(path)
            records.append({'id':category+'_'+str(part+1),'title':LABELS[category], 'file':path.relative_to(REVIEW).as_posix()})
    return records


def build():
    for sub in ('faces','thumbs','contacts','screens','evidence'):
        (REVIEW/sub).mkdir(parents=True,exist_ok=True)
    catalog=read(ART/'catalog.json')
    approval=read(ART/'approval.json',{})
    manifest={r['id']:r for r in read(ROOT/'art/manifest.json')['assets']}
    faces=face_info()
    assets=[]
    for key,row in catalog['assets'].items():
        source=ROOT/row['file']
        size=list(Image.open(source).size)
        thumb=REVIEW/'thumbs'/(key.replace('.','_')+'.png')
        thumbnail(source,thumb)
        a={'id':key,'title':manifest[key]['title'],'category':row['category'],'category_zh':LABELS[row['category']],
           'file':row['file'],'url':'../../../'+row['file'].removeprefix('art/'),
           'thumb':thumb.relative_to(REVIEW).as_posix(),'size':size,
           'sha256':digest(source),'retained':row['status']=='retained',
           'face_edit':row.get('has_face_override',False),'face_protection':row.get('face_protection')}
        a['user_approval']='approved' if approval.get('assets',{}).get(key)==a['sha256'] else 'pending'
        # REVIEW is art/density3/review: back to repo via ../../../ .
        a['url']='../../../'+row['file']
        if key in faces:
            a['face']=crop_faces(row,faces[key])
            # Derivatives are under repo assets/, three levels above review/.
            a['face']['portrait']=a['face']['portrait'].replace('../../../../','../../../')
            a['face']['queue']=a['face']['queue'].replace('../../../../','../../../')
        assets.append(a)
    screen_rows=[]
    game=WORK/'agent_reviews/integration/final_game'
    index=read(game/'capture-index.json',[])
    if isinstance(index,dict):
        index=index.get('captures',index.get('items',[]))
    for row in index:
        source=game/row['file']
        destination=REVIEW/'screens'/source.name
        shutil.copy2(source,destination)
        thumb=REVIEW/'thumbs'/('screen_'+source.stem+'.png')
        thumbnail(source,thumb,(320,180))
        screen_rows.append({**row,'file':destination.relative_to(REVIEW).as_posix(),'thumb':thumb.relative_to(REVIEW).as_posix(),
                            'sha256':digest(destination),
                            'user_approval':'approved' if approval.get('screens',{}).get(row['id'])==digest(destination) else 'pending',
                            'note_zh':('Master Restorer 的长描述会越过固定卡片底边。这是已有文字排版问题；本次未修改字体或卡片尺寸。' if row['id']=='upgrade_cards' else
                                       'Gerda 的长名字超出固定对话框右边缘，沿用既有字体与框宽；人物脸部及头像完整。' if row['id']=='figure_gerda' else '')})
    evidence=[]
    for team in ('a','b'):
        folder=WORK/f'agent_reviews/{team}'
        for name in ('final_3px_review.json','final_verified_review.json','final_derived_heads_review.json',
                     'final_game_face_review.json','face_protection_'+team+'.json',
                     'fennick_edit_record.json','fennick_edit_prompt.txt','silas_edit_record.json','silas_edit_prompt.txt'):
            source=folder/name
            if source.is_file():
                target=REVIEW/'evidence'/f'{team}_{name}'
                shutil.copy2(source,target)
                evidence.append(target.relative_to(REVIEW).as_posix())
    for name in ('runtime-audit.json','skrae_crop_audit.json','size_override_report.json'):
        source=WORK/'agent_reviews/integration'/name
        if source.is_file():
            target=REVIEW/'evidence'/name
            shutil.copy2(source,target)
            evidence.append(target.relative_to(REVIEW).as_posix())
    approved=all(a['user_approval']=='approved' for a in assets+screen_rows)
    data={'version':1,'user_approval':'approved' if approved else 'pending',
          'approval_recorded_at':approval.get('recorded_at_utc'),
          'reference':[2560,1440],'pitch':3,
          'assets':assets,'screens':screen_rows,'contacts':contacts(assets),'evidence':evidence,
          'counts':{'assets':len(assets),'processed':sum(not a['retained'] for a in assets),'retained':sum(a['retained'] for a in assets),
                    'figures':sum(a['category']=='figure' for a in assets),'screens':len(screen_rows)}}
    (REVIEW/'data.json').write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf-8')
    (REVIEW/'data.js').write_text('window.REVIEW_DATA='+json.dumps(data,ensure_ascii=False)+';\n',encoding='utf-8')
    entry = ROOT.parents[2] / '3px美术完整审核.html'
    template = (REVIEW/'index.html').read_text(encoding='utf-8')
    entry.write_text(template.replace('<head>', '<head>\n<base href=".本地化/art-round2/density-3px-review/art/density3/review/">'), encoding='utf-8')
    print(json.dumps(data['counts']))


if __name__=='__main__':
    build()
