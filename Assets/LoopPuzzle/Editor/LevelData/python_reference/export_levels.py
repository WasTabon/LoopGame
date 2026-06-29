"""Export validated levels to JSON for the Unity editor importer."""
import json
from levels_def import LEVELS
from validate_levels import validate

# validate (also computes parMoves) and abort if any fail
failed=[]
for lv in LEVELS:
    errs=validate(lv)
    if errs: failed.append((lv['name'],errs))
if failed:
    print("VALIDATION FAILED, not exporting:")
    for n,e in failed: print(n,e)
    raise SystemExit(1)

PT_INT={"None":0,"Straight":1,"Corner":2,"Triple":3,"Cross":4}
CT_INT={"Empty":0,"Fixed":1,"Movable":2,"Obstacle":3,"Start":4}

out=[]
for lv in LEVELS:
    w,h=lv['w'],lv['h']
    start=lv['start']; fixed=lv['fixed']; obs=lv['obstacles']
    solution=lv['solution']; displaced=lv['displaced']
    # pick start cell: the cell that maps to solution start anchor.
    # Use the lowest-index occupied non-displaced solution cell as the start marker.
    # Simpler: start marker = a movable piece cell that is in the solution (on-place).
    on_place=[c for c in start if c not in lv['displaced']]
    # prefer a fixed-free on-place cell; fallback any
    start_marker=sorted(on_place)[0]
    cells=[]
    for y in range(h):
        for x in range(w):
            c=(x,y)
            if c in obs:
                cells.append({"ct":CT_INT["Obstacle"],"pt":0,"rot":0,"start":False})
            elif c in start:
                pt,r=start[c]
                ct = "Fixed" if c in fixed else "Movable"
                cells.append({"ct":CT_INT[ct],"pt":PT_INT[pt],"rot":r,
                              "start": (c==start_marker)})
            else:
                cells.append({"ct":CT_INT["Empty"],"pt":0,"rot":0,"start":False})

    # Build solution as a list of target cells: each final cell needs a piece type + rotation.
    # Hint logic compares the CURRENT board against this (works regardless of player moves).
    sol_entries=[]
    for tcell,(pt,r) in solution.items():
        sol_entries.append({"tx":tcell[0],"ty":tcell[1],
                            "pt":PT_INT[pt],"trot":r})

    out.append({
        "name":lv['name'],"world":lv['world'],"levelNumber":int(lv['name'].split('_')[1]),
        "width":w,"height":h,"requiredLoops":lv['required_loops'],
        "parMoves":lv['parMoves'],"cells":cells,"solution":sol_entries
    })

with open("levels_export.json","w") as f:
    json.dump(out,f,indent=1)
print(f"Exported {len(out)} validated levels to levels_export.json")
# sanity: ensure each level has exactly one start marker
for lv in out:
    sc=sum(1 for c in lv['cells'] if c['start'])
    assert sc==1, f"{lv['name']} has {sc} start markers"
print("Each level has exactly one start marker.")
