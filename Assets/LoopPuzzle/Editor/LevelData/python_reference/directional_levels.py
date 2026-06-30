"""Iteration 14 demo levels (32-34): directional (arrow) pieces. Flow must pass
through each directional piece in its arrow's direction. Validated:
  - solution forms a loop AND all arrows consistent with one traversal orientation
  - start is unsolved (loop broken OR arrows inconsistent)
  - reconstruction from start (rotating to solution) wins
"""
import json
from level_engine import count_closed_loops, rot as ROT, trace_exit_directions, is_directional_win, min_taps

PT_INT={"None":0,"Straight":1,"Corner":2,"Triple":3,"Cross":4,"Bridge":5}
CT_INT={"Empty":0,"Fixed":1,"Movable":2,"Obstacle":3,"Start":4,"Portal":5,"Breakable":6}
OPP={0:2,1:3,2:0,3:1}

def corner_for(dirs):
    dirs=frozenset(dirs)
    for r in range(4):
        if frozenset(d for d in range(4) if ROT("Corner",r)[d])==dirs: return ("Corner",r)
    raise ValueError(dirs)

def straight_for(dirs):
    dirs=frozenset(dirs)
    for r in range(4):
        if frozenset(d for d in range(4) if ROT("Straight",r)[d])==dirs: return ("Straight",r)
    raise ValueError(dirs)

def square_loop(ox,oy):
    return {
        (ox+0,oy+0): corner_for({0,1}),
        (ox+1,oy+0): corner_for({0,3}),
        (ox+1,oy+1): corner_for({2,3}),
        (ox+0,oy+1): corner_for({1,2}),
    }

def rect_loop(ox,oy,w,h):
    cells={}
    for x in range(ox,ox+w):
        for y in range(oy,oy+h):
            if not (x==ox or x==ox+w-1 or y==oy or y==oy+h-1): continue
            if x==ox and y==oy: dirs={0,1}
            elif x==ox+w-1 and y==oy: dirs={0,3}
            elif x==ox+w-1 and y==oy+h-1: dirs={2,3}
            elif x==ox and y==oy+h-1: dirs={1,2}
            elif y==oy or y==oy+h-1: dirs={1,3}
            else: dirs={0,2}
            cells[(x,y)]=straight_for(dirs) if (dirs=={0,2} or dirs=={1,3}) else corner_for(dirs)
    return cells

LEVELS=[]

def make_level(name, world, w, h, sol, arrow_cells, scramble_offset=1):
    """arrow_cells: list of positions that get arrows matching the solution traversal.
    Directional pieces become a real constraint when they're connector-symmetric
    (e.g. Straight: rot and rot+2 give same connectors but opposite arrow), forcing
    the player to pick the unique arrow-correct orientation."""
    exit_of=trace_exit_directions(sol)
    assert exit_of is not None, "solution not a simple loop"
    arrows_abs={pos:exit_of[pos] for pos in arrow_cells}
    assert is_directional_win(sol, arrows_abs), "solution not directional-win"
    start={}; arrowbase={}
    for pos,(pt,r) in sol.items():
        sr=(r-scramble_offset)%4
        start[pos]=(pt,sr)
        if pos in arrows_abs:
            arrowbase[pos]=(arrows_abs[pos]-r)%4
    start_arrows={pos:(arrowbase[pos]+start[pos][1])%4 for pos in arrowbase}
    assert not is_directional_win({p:start[p] for p in start}, start_arrows), "start already solved"
    LEVELS.append(dict(name=name,world=world,w=w,h=h,sol=sol,start=start,
                       arrowbase=arrowbase,arrows_abs=arrows_abs,scramble=scramble_offset))

# L32: 3x3 ring, 2 directional straights on opposite edges (must agree on flow direction)
sol=rect_loop(0,0,3,3)
make_level("Level_32",5,3,3,sol,[(1,0),(1,2)],scramble_offset=2)

# L33: 3x3 ring, all 4 edge straights directional (strong constraint)
sol=rect_loop(0,0,3,3)
make_level("Level_33",5,3,3,sol,[(1,0),(1,2),(0,1),(2,1)],scramble_offset=2)

# L34: 3x4 ring with directional straights
sol=rect_loop(0,0,3,4)
# pick the straight edge pieces (non-corner) for arrows
straight_positions=[p for p in sol if sol[p][0]=='Straight']
make_level("Level_34",5,3,4,sol,straight_positions[:3],scramble_offset=1)

def validate(lv):
    errs=[]
    sol=lv['sol']; start=lv['start']
    if count_closed_loops(sol)[0]!=1: errs.append("solution not 1 loop")
    if not is_directional_win(sol, lv['arrows_abs']): errs.append("solution not directional-win")
    start_arrows={pos:(lv['arrowbase'][pos]+start[pos][1])%4 for pos in lv['arrowbase']}
    if is_directional_win({p:start[p] for p in start}, start_arrows): errs.append("start solved")
    # reconstruction: rotate each to solution rotation
    recon={pos:(start[pos][0], sol[pos][1]) for pos in start}
    recon_arrows={pos:(lv['arrowbase'][pos]+sol[pos][1])%4 for pos in lv['arrowbase']}
    if not is_directional_win(recon, recon_arrows): errs.append("reconstruction fails")
    for p in sol:
        if not (0<=p[0]<lv['w'] and 0<=p[1]<lv['h']): errs.append(f"OOB {p}")
    return errs

def export():
    out=[]
    for lv in LEVELS:
        errs=validate(lv)
        if errs:
            print(f"FAIL {lv['name']}: {errs}"); return None
        w,h=lv['w'],lv['h']; sol=lv['sol']; start=lv['start']; arrowbase=lv['arrowbase']
        marker=sorted(start)[0]
        par=sum(min_taps(start[p][1], sol[p][1]) for p in start)
        cells=[]
        for y in range(h):
            for x in range(w):
                c=(x,y)
                if c in start:
                    pt,r=start[c]
                    is_dir = c in arrowbase
                    cells.append({"ct":CT_INT["Movable"],"pt":PT_INT[pt],"rot":r,
                                  "start":(c==marker),"color":0,"portalId":0,
                                  "maxRotations":0,"directional":is_dir,
                                  "arrowDir":arrowbase.get(c,0)})
                else:
                    cells.append({"ct":CT_INT["Empty"],"pt":0,"rot":0,"start":False,
                                  "color":0,"portalId":0,"maxRotations":0,
                                  "directional":False,"arrowDir":0})
        soldata=[{"tx":p[0],"ty":p[1],"pt":PT_INT[sol[p][0]],"trot":sol[p][1],"color":0}
                 for p in sol]
        out.append({"name":lv['name'],"world":lv['world'],
                    "levelNumber":int(lv['name'].split('_')[1]),
                    "width":w,"height":h,"requiredLoops":1,"parMoves":par,
                    "colorLoopsMode":False,"coverAllMode":False,"directionalMode":True,
                    "cells":cells,"solution":soldata})
    return out

if __name__=="__main__":
    data=export()
    if data:
        json.dump(data, open("directional_levels_export.json","w"), indent=1)
        print(f"Exported {len(data)} directional levels.")
        for lv in data:
            nd=sum(1 for c in lv['cells'] if c['directional'])
            print(f"  {lv['name']}: {lv['width']}x{lv['height']}, par {lv['parMoves']}, {nd} arrows")
