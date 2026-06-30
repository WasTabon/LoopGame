"""Iteration 13 demo levels (29-31): rotation-limit and breakable-cell mechanics.
Each level is constructed and validated:
  - solution forms required loops
  - start is unsolved
  - rotation-limit levels: solution reachable within each piece's maxRotations
  - breakable-cell levels: a valid move ORDER exists (breaking is irreversible),
    and the solution does not require the broken cell afterward.
"""
import json
from level_engine import count_closed_loops, rot as ROT, min_taps

PT_INT={"None":0,"Straight":1,"Corner":2,"Triple":3,"Cross":4,"Bridge":5}
CT_INT={"Empty":0,"Fixed":1,"Movable":2,"Obstacle":3,"Start":4,"Portal":5,"Breakable":6}

# Correct corner rotations (verified):
#   rot0={N,E} rot1={E,S} rot2={S,W} rot3={W,N}
def corner_for(dirs):
    dirs=frozenset(dirs)
    for r in range(4):
        c=ROT("Corner",r)
        if frozenset(d for d in range(4) if c[d])==dirs: return ("Corner",r)
    raise ValueError(dirs)

def straight_for(dirs):
    dirs=frozenset(dirs)
    for r in range(4):
        c=ROT("Straight",r)
        if frozenset(d for d in range(4) if c[d])==dirs: return ("Straight",r)
    raise ValueError(dirs)

def square_loop(ox, oy):
    """2x2 corner loop at offset. Returns {pos:(pt,rot)}."""
    return {
        (ox+0,oy+0): corner_for({0,1}),   # bottom-left: N,E
        (ox+1,oy+0): corner_for({0,3}),   # bottom-right: N,W
        (ox+1,oy+1): corner_for({2,3}),   # top-right: S,W
        (ox+0,oy+1): corner_for({1,2}),   # top-left: E,S
    }

def rect_loop(ox, oy, w, h):
    """Rectangular loop perimeter w x h (w,h>=2). Returns {pos:(pt,rot)}."""
    cells={}
    for x in range(ox, ox+w):
        for y in range(oy, oy+h):
            on_perim = (x==ox or x==ox+w-1 or y==oy or y==oy+h-1)
            if not on_perim: continue
            dirs=set()
            # connect along perimeter
            if x>ox and (y==oy or y==oy+h-1): dirs.add(3)  # W
            if x<ox+w-1 and (y==oy or y==oy+h-1): dirs.add(1)  # E
            if y>oy and (x==ox or x==ox+w-1): dirs.add(2)  # S
            if y<oy+h-1 and (x==ox or x==ox+w-1): dirs.add(0)  # N
            # corners need exactly two perpendicular
            if x==ox and y==oy: dirs={0,1}
            elif x==ox+w-1 and y==oy: dirs={0,3}
            elif x==ox+w-1 and y==oy+h-1: dirs={2,3}
            elif x==ox and y==oy+h-1: dirs={1,2}
            if len(dirs)==2 and (dirs=={0,2} or dirs=={1,3}):
                cells[(x,y)]=straight_for(dirs)
            else:
                cells[(x,y)]=corner_for(dirs)
    return cells

LEVELS=[]

# ---------- L29: rotation-limit, 2x2 square ----------
sol = square_loop(0,0)
assert count_closed_loops(sol)==(1,True), count_closed_loops(sol)
# scramble each corner so it needs 1-2 rotations; maxRotations = exact taps needed
scr = {(0,0):1,(1,0):2,(1,1):1,(0,1):2}  # CW rotations needed to reach solved
start={}; maxrot={}
for pos,(pt,r) in sol.items():
    need=scr[pos]
    sr=(r-need)%4
    start[pos]=(pt,sr)
    maxrot[pos]=need
assert count_closed_loops(start)[0]!=1
LEVELS.append(dict(name="Level_29",world=5,w=3,h=3,sol=sol,start=start,
                   maxrot=maxrot,breakable=set(),order=None))

# ---------- L30: rotation-limit, 3x3 ring (8 pieces) ----------
sol = rect_loop(0,0,3,3)
assert count_closed_loops(sol)==(1,True), count_closed_loops(sol)
scr = {pos:((hash(pos)%2)+1) for pos in sol}  # 1-2 rotations each
start={}; maxrot={}
for pos,(pt,r) in sol.items():
    need=scr[pos]
    sr=(r-need)%4
    start[pos]=(pt,sr)
    maxrot[pos]=need
assert count_closed_loops(start)[0]!=1
LEVELS.append(dict(name="Level_30",world=5,w=3,h=3,sol=sol,start=start,
                   maxrot=maxrot,breakable=set(),order=None))

# ---------- L31: breakable cell ----------
# Design: a 2x2 loop at (0,0). One piece STARTS on a breakable cell at (2,0) and must
# be dragged to its solution cell. After it leaves, (2,0) breaks - and the solution
# does NOT use (2,0). So a valid order exists trivially (just drag it once).
# To make it meaningful: there's a piece on breakable (2,0) that must move to (0,1)
# (an empty solution slot). The 2x2 loop needs 4 corners; 3 are in place (movable,
# scrambled rotation) and the 4th starts displaced on the breakable cell.
sol = square_loop(0,0)
# The displaced piece is the one for (0,1) = corner {E,S}. It starts at (2,0) on breakable.
displaced_target = (0,1)
displaced_piece = sol[displaced_target]  # ('Corner', rot for {1,2})
# start: 3 corners in place (scrambled so they need only 1 rotation), 4th on breakable cell
start={}
maxrot={}
for pos,(pt,r) in sol.items():
    if pos==displaced_target:
        continue
    start[pos]=(pt,(r-1)%4)  # needs exactly 1 CW rotation to solve
    maxrot[pos]=0
# displaced piece on breakable (2,0), needs 1 rotation after placing
start[(2,0)] = (displaced_piece[0], (displaced_piece[1]-1)%4)
maxrot[(2,0)] = 0
breakable = {(2,0)}
LEVELS.append(dict(name="Level_31",world=5,w=3,h=2,sol=sol,start=start,
                   maxrot=maxrot,breakable=breakable,order="drag-then-rotate",
                   displaced={(2,0):(0,1)}))

def validate(lv):
    errs=[]
    sol=lv['sol']; start=lv['start']
    if count_closed_loops(sol)!=(1,True):
        errs.append(f"solution not 1 loop: {count_closed_loops(sol)}")
    if count_closed_loops(start)[0]==1:
        errs.append("start already solved")
    # rotation-limit feasibility: for non-displaced pieces, taps needed <= maxRotations
    displaced = lv.get('displaced',{})
    for pos,(pt,r) in sol.items():
        # find where this piece starts
        if pos in displaced.values():
            # it's the displaced target; piece starts at the breakable source
            src=[s for s,d in displaced.items() if d==pos][0]
            spt,sr=start[src]
        elif pos in start:
            spt,sr=start[pos]
        else:
            errs.append(f"no start for {pos}"); continue
        if spt!=pt:
            errs.append(f"type mismatch at {pos}: {spt} vs {pt}")
        need=min_taps(sr,r)
        lim=lv['maxrot'].get(pos if pos in lv['maxrot'] else src,0)
        if lim>0 and need>lim:
            errs.append(f"piece {pos} needs {need} rotations but limit {lim}")
    # breakable feasibility: solution must not place a piece on a breakable source cell
    for b in lv['breakable']:
        if b in sol:
            errs.append(f"solution uses breakable cell {b} (would be broken)")
    # bounds
    for p in sol:
        if not (0<=p[0]<lv['w'] and 0<=p[1]<lv['h']):
            errs.append(f"OOB sol {p}")
    return errs

def export():
    out=[]
    for lv in LEVELS:
        errs=validate(lv)
        if errs:
            print(f"FAIL {lv['name']}: {errs}"); return None
        w,h=lv['w'],lv['h']; sol=lv['sol']; start=lv['start']
        displaced=lv.get('displaced',{})
        # build cell grid from START positions
        # determine start marker: a cell that is in the final loop and present at start
        loop_starts=[p for p in start if p in sol]
        start_marker=sorted(loop_starts)[0] if loop_starts else sorted(start)[0]
        # par = total rotations needed + drags
        par=0
        for pos,(pt,r) in sol.items():
            if pos in displaced.values():
                src=[s for s,d in displaced.items() if d==pos][0]
                par+=min_taps(start[src][1], r)
            elif pos in start:
                par+=min_taps(start[pos][1], r)
        par+=len(displaced)  # one drag each
        cells=[]
        for y in range(h):
            for x in range(w):
                c=(x,y)
                if c in start:
                    pt,r=start[c]
                    ct = "Breakable" if c in lv['breakable'] else "Movable"
                    cells.append({"ct":CT_INT[ct],"pt":PT_INT[pt],"rot":r,
                                  "start":(c==start_marker),"color":0,"portalId":0,
                                  "maxRotations":lv['maxrot'].get(c,0),"directional":False})
                else:
                    cells.append({"ct":CT_INT["Empty"],"pt":0,"rot":0,"start":False,
                                  "color":0,"portalId":0,"maxRotations":0,"directional":False})
        soldata=[{"tx":p[0],"ty":p[1],"pt":PT_INT[sol[p][0]],"trot":sol[p][1],"color":0}
                 for p in sol]
        out.append({"name":lv['name'],"world":lv['world'],
                    "levelNumber":int(lv['name'].split('_')[1]),
                    "width":w,"height":h,"requiredLoops":1,"parMoves":par,
                    "colorLoopsMode":False,"coverAllMode":False,
                    "cells":cells,"solution":soldata})
    return out

if __name__=="__main__":
    data=export()
    if data:
        json.dump(data, open("breakable_levels_export.json","w"), indent=1)
        print(f"Exported {len(data)} iteration-13 levels.")
        for lv in data:
            mech = "breakable" if any(c['ct']==CT_INT['Breakable'] for c in lv['cells']) else "rotation-limit"
            print(f"  {lv['name']}: {lv['width']}x{lv['height']}, par {lv['parMoves']}, {mech}")
