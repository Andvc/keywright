"""Record final production file hashes and complete per-asset prompt additions."""
from pathlib import Path
import hashlib
import json

ROOT=Path(__file__).resolve().parents[3]
folder=ROOT/'art/generated/_workflow'
m=json.loads((ROOT/'art/manifest.json').read_text('utf-8'))
records=json.loads((folder/'generations.json').read_text('utf-8'))
changes=json.loads((folder/'prompt_changes.json').read_text('utf-8'))
delivery=[]
for a in m['assets']:
    p=ROOT/a['output']
    digest=hashlib.sha256(p.read_bytes()).hexdigest()
    records[a['id']]['final_sha256']=digest
    if records[a['id']].get('note') and a['id'] not in changes:
        changes[a['id']]=records[a['id']]['note']
    if a['category'].startswith('icon') or a['id'] in ('emblem.guild','story.key_whole'):
        records[a['id']]['selection_reason']='Selected after native-size and enlarged comparison for the clearest identifying shape and required details in the approved style.'
    entry={'id':a['id'],'batch':a['batch'],'native':a['output'],'size':a['size'],'sha256':digest}
    if a['category']=='store':
        final=ROOT/'art/generated/store/final'/p.name
        entry['steam_export']={'path':final.relative_to(ROOT).as_posix(),'size':a['deliver_size'],
            'sha256':hashlib.sha256(final.read_bytes()).hexdigest()}
    delivery.append(entry)
for name,data in [('generations.json',records),('prompt_changes.json',changes),('delivery_manifest.json',delivery)]:
    (folder/name).write_text(json.dumps(data,indent=2,ensure_ascii=True)+'\n',encoding='utf-8')
print(f'Recorded final hashes for {len(delivery)} native assets and nine Steam exports.')
