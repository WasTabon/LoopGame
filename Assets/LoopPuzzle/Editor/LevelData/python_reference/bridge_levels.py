"""Bridge mechanic demo levels (iteration 12). Adds levels 26-28 using the Bridge
piece (figure-8 crossover). Each is constructed and validated: solution wins,
start is unsolved, reconstruction from start wins, piece types preserved."""
import json
from collections import defaultdict
from level_engine import count_closed_loops, rot as ROT

PT_INT={"None":0,"Straight":1,"Corner":2,"Triple":3,"Cross":4,"Bridge":5}
CT_INT={"Empty":0,"Fixed":1,"Movable":2,"Obstacle":3,"Start":4,"Portal":5,"Breakable":6}

def piece_for(conns):
    conns=frozenset(conns)
    for pt in ("Straight","Corner"):
        for r in range(4):
            c=ROT(pt,r)
            if frozenset(d for d in range(4) if c[d])==conns: return (pt,r)
    raise ValueError(f"no piece for {conns}")

def _db(a,b):
    dx,dy=b[0]-a[0],b[1]-a[1]
    return {(0,1):0,(1,0):1,(0,-1):2,(-1,0):3}[(dx,dy)]

def build_fig8_bridge(ox, oy):
    bx,by=ox+2,oy+2
    segA=[(ox+3,oy+2),(ox+4,oy+2),(ox+4,oy+1),(ox+4,oy+0),(ox+3,oy+0),(ox+2,oy+0),(ox+2,oy+1)]
    segB=[(ox+2,oy+3),(ox+2,oy+4),(ox+1,oy+4),(ox+0,oy+4),(ox+0,oy+3),(ox+0,oy+2),(ox+1,oy+2)]
    order=[(bx,by)]+segA+[(bx,by)]+segB
    cd=defaultdict(set)
    n=len(order)
    for i in range(n):
        a=order[i]; b=order[(i+1)%n]
        cd[a].add(_db(a,b)); cd[b].add(_db(b,a))
    placement={}
    for cell,dirs in cd.items():
        if cell==(bx,by):
            assert dirs=={0,1,2,3}
            placement[cell]=('Bridge',0)
        else:
            placement[cell]=piece_for(dirs)
    nl,ok=count_closed_loops(placement)
    assert ok and nl==1, f"build failed {nl},{ok}"
    return placement

def scramble(sol, offset):
    out={}
    for pos,(pt,r) in sol.items():
        out[pos]=(pt,r) if pt=='Bridge' else (pt,(r+offset)%4)
    return out

def min_taps(s,t): return (t-s)%4

LEVELS=[]
def add(name, world, w, h, sol, start, offset_used):
    LEVELS.append(dict(name=name, world=world, w=w, h=h, solution=sol, start=start))

# L26: basic figure-8 bridge, 5x5
sol=build_fig8_bridge(0,0)
add("Level_26",5,5,5,sol,scramble(sol,1),1)

# L27: figure-8 bridge shifted on 6x6 (more empty space, harder to read)
sol=build_fig8_bridge(0,0)
# pad to 6x6 by just using 6x6 board with same shape
add("Level_27",5,6,6,sol,scramble(sol,2),2)

# L28: figure-8 bridge on 6x6 with offset position
sol=build_fig8_bridge(1,1)
add("Level_28",5,6,6,sol,scramble(sol,3),3)

def validate(lv):
    errs=[]
    sol=lv['solution']; start=lv['start']
    nl,ok=count_closed_loops(sol)
    if not (ok and nl==1): errs.append(f"solution not 1 loop: {nl},{ok}")
    ns,_=count_closed_loops(start)
    if ns==1: errs.append("start already solved")
    # piece types preserved
    import collections
    ts=collections.Counter(pt for pt,_ in start.values())
    to=collections.Counter(pt for pt,_ in sol.values())
    if ts!=to: errs.append(f"type mismatch {dict(ts)} vs {dict(to)}")
    # reconstruction: apply solution rotations to start positions (no displaced here)
    recon={pos:(start[pos][0], sol[pos][1]) for pos in start}
    nr,okr=count_closed_loops(recon)
    if not (okr and nr==1): errs.append(f"reconstruction fails: {nr},{okr}")
    # bounds
    for p in sol:
        if not (0<=p[0]<lv['w'] and 0<=p[1]<lv['h']): errs.append(f"OOB {p}")
    # par
    par=sum(min_taps(start[p][1], sol[p][1]) for p in start if start[p][0]!='Bridge')
    lv['parMoves']=par
    return errs

def export():
    out=[]
    for lv in LEVELS:
        errs=validate(lv)
        if errs:
            print(f"FAIL {lv['name']}: {errs}"); return None
        w,h=lv['w'],lv['h']; sol=lv['solution']; start=lv['start']
        # start marker = lowest-index non-bridge cell
        non_bridge=sorted(c for c in start if start[c][0]!='Bridge')
        start_marker=non_bridge[0]
        cells=[]
        for y in range(h):
            for x in range(w):
                c=(x,y)
                if c in start:
                    pt,r=start[c]
                    cells.append({"ct":CT_INT["Movable"],"pt":PT_INT[pt],"rot":r,
                                  "start":(c==start_marker),"color":0,"portalId":0,
                                  "maxRotations":0,"directional":False})
                else:
                    cells.append({"ct":CT_INT["Empty"],"pt":0,"rot":0,"start":False,
                                  "color":0,"portalId":0,"maxRotations":0,"directional":False})
        soldata=[{"tx":p[0],"ty":p[1],"pt":PT_INT[sol[p][0]],"trot":sol[p][1],"color":0}
                 for p in sol]
        out.append({"name":lv['name'],"world":lv['world'],
                    "levelNumber":int(lv['name'].split('_')[1]),
                    "width":w,"height":h,"requiredLoops":1,"parMoves":lv['parMoves'],
                    "colorLoopsMode":False,"coverAllMode":False,
                    "cells":cells,"solution":soldata})
    return out

if __name__=="__main__":
    data=export()
    if data:
        json.dump(data, open("bridge_levels_export.json","w"), indent=1)
        print(f"Exported {len(data)} bridge levels.")
        for lv in data:
            print(f"  {lv['name']}: {lv['width']}x{lv['height']}, par {lv['parMoves']}")
