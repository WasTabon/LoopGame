"""Iteration 15 demo levels (35-37): Color Loops. Multiple colored piece sets, each
must form its own closed same-color loop (colors cannot mix). Validated:
  - solution: every piece in a closed loop of its own color, no open ends
  - start is unsolved
  - reconstruction (rotate to solution) wins
"""
import json
from level_engine import count_closed_loops, is_color_win, rot as ROT, min_taps

PT_INT={"None":0,"Straight":1,"Corner":2,"Triple":3,"Cross":4,"Bridge":5}
CT_INT={"Empty":0,"Fixed":1,"Movable":2,"Obstacle":3,"Start":4,"Portal":5,"Breakable":6}
COLOR_INT={"Neutral":0,"Red":1,"Blue":2,"Green":3,"Yellow":4}

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

def square_loop(ox,oy,color):
    """2x2 corner loop with a color. Returns {pos:(pt,rot,color)}."""
    base={
        (ox+0,oy+0): corner_for({0,1}),
        (ox+1,oy+0): corner_for({0,3}),
        (ox+1,oy+1): corner_for({2,3}),
        (ox+0,oy+1): corner_for({1,2}),
    }
    return {p:(pt,r,color) for p,(pt,r) in base.items()}

def rect_loop(ox,oy,w,h,color):
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
            pt = straight_for(dirs) if (dirs=={0,2} or dirs=={1,3}) else corner_for(dirs)
            cells[(x,y)]=(pt[0],pt[1],color)
    return cells

LEVELS=[]

def add_level(name, world, w, h, sol, scramble=1):
    """sol: {pos:(pt,rot,color)}. Build scrambled start; verify color-win logic."""
    assert is_color_win(sol), f"{name}: solution not color-win: {count_closed_loops(sol)}"
    start={}
    for pos,(pt,r,col) in sol.items():
        start[pos]=(pt,(r-scramble)%4,col)
    assert not is_color_win({p:start[p] for p in start}), f"{name}: start already solved"
    LEVELS.append(dict(name=name,world=world,w=w,h=h,sol=sol,start=start,scramble=scramble))

# L35: two 2x2 loops, red + blue, side by side on 5x2
sol={**square_loop(0,0,1), **square_loop(3,0,2)}
add_level("Level_35",5,5,2,sol,scramble=1)

# L36: red 2x2 + blue 2x2 stacked on 2x5
sol={**square_loop(0,0,1), **square_loop(0,3,2)}
add_level("Level_36",5,2,5,sol,scramble=2)

# L37: three colors - red, blue, green 2x2 loops on 5x5 (spread out)
sol={**square_loop(0,0,1), **square_loop(3,0,2), **square_loop(0,3,3)}
add_level("Level_37",5,5,5,sol,scramble=1)

def validate(lv):
    errs=[]
    sol=lv['sol']; start=lv['start']
    if not is_color_win(sol): errs.append(f"solution not color-win: {count_closed_loops(sol)}")
    if is_color_win({p:start[p] for p in start}): errs.append("start already solved")
    recon={pos:(start[pos][0], sol[pos][1], start[pos][2]) for pos in start}
    if not is_color_win(recon): errs.append("reconstruction fails")
    for p in sol:
        if not (0<=p[0]<lv['w'] and 0<=p[1]<lv['h']): errs.append(f"OOB {p}")
    return errs

def export():
    out=[]
    for lv in LEVELS:
        errs=validate(lv)
        if errs:
            print(f"FAIL {lv['name']}: {errs}"); return None
        w,h=lv['w'],lv['h']; sol=lv['sol']; start=lv['start']
        marker=sorted(start)[0]
        par=sum(min_taps(start[p][1], sol[p][1]) for p in start)
        cells=[]
        for y in range(h):
            for x in range(w):
                c=(x,y)
                if c in start:
                    pt,r,col=start[c]
                    cells.append({"ct":CT_INT["Movable"],"pt":PT_INT[pt],"rot":r,
                                  "start":(c==marker),"color":col,"portalId":0,
                                  "maxRotations":0,"directional":False,"arrowDir":0})
                else:
                    cells.append({"ct":CT_INT["Empty"],"pt":0,"rot":0,"start":False,
                                  "color":0,"portalId":0,"maxRotations":0,
                                  "directional":False,"arrowDir":0})
        soldata=[{"tx":p[0],"ty":p[1],"pt":PT_INT[sol[p][0]],"trot":sol[p][1],"color":sol[p][2]}
                 for p in sol]
        out.append({"name":lv['name'],"world":lv['world'],
                    "levelNumber":int(lv['name'].split('_')[1]),
                    "width":w,"height":h,"requiredLoops":1,"parMoves":par,
                    "colorLoopsMode":True,"coverAllMode":False,"directionalMode":False,
                    "cells":cells,"solution":soldata})
    return out

if __name__=="__main__":
    data=export()
    if data:
        json.dump(data, open("color_levels_export.json","w"), indent=1)
        print(f"Exported {len(data)} color-loop levels.")
        for lv in data:
            colors=len(set(c['color'] for c in lv['cells'] if c['color']>0))
            print(f"  {lv['name']}: {lv['width']}x{lv['height']}, par {lv['parMoves']}, {colors} colors")
