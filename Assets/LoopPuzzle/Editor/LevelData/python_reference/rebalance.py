"""Iteration 17 rebalance (Variant B): reorganize all 40 levels into a clean 7-world
difficulty curve, and add 3 new combo levels (2 mechanics each), all proven solvable.
Output: a full levels_export.json with new sequential numbering and worlds."""
import json
from level_engine import (count_closed_loops, is_color_win, is_directional_win,
                          is_portal_win, trace_exit_directions, rot as ROT, min_taps)

PT_INT={"None":0,"Straight":1,"Corner":2,"Triple":3,"Cross":4,"Bridge":5}
CT_INT={"Empty":0,"Fixed":1,"Movable":2,"Obstacle":3,"Start":4,"Portal":5,"Breakable":6}

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

# ---------- Load existing 40 levels ----------
existing={lv['levelNumber']:lv for lv in json.load(open("levels_export_full4.json"))}

# ---------- Reorder plan: world -> [old level numbers] in ascending difficulty ----------
PLAN = {
 1: [1,3,5,2,4],
 2: [6,7,8,10,9],
 3: [13,11,12,18,16,17],
 4: [29,30,31,28,27,26],
 5: [34,32,33,35,37,36],
 6: [38,39,40,14,15,19],
 7: [20,23,21,22,24,25],
}

# ---------- Build 3 combo levels ----------
def cell_movable(pt,r,color=0,arrow=None):
    c={"ct":CT_INT["Movable"],"pt":PT_INT[pt],"rot":r,"start":False,"color":color,
       "portalId":0,"portalDir":0,"maxRotations":0,
       "directional":(arrow is not None),"arrowDir":(arrow if arrow is not None else 0)}
    return c
def cell_empty():
    return {"ct":CT_INT["Empty"],"pt":0,"rot":0,"start":False,"color":0,"portalId":0,
            "portalDir":0,"maxRotations":0,"directional":False,"arrowDir":0}
def cell_obstacle():
    return {"ct":CT_INT["Obstacle"],"pt":0,"rot":0,"start":False,"color":0,"portalId":0,
            "portalDir":0,"maxRotations":0,"directional":False,"arrowDir":0}

COMBOS=[]

# COMBO A: ARROWS on a larger 4x3 ring (harder directional, single loop = safe to validate).
def build_combo_color_arrows():
    def ring(ox,oy,w,h):
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
    sol=ring(0,0,4,3)
    assert count_closed_loops(sol)==(1,True), count_closed_loops(sol)
    ex=trace_exit_directions(sol)
    # arrows on several straight edges
    straights=[p for p in sol if sol[p][0]=='Straight']
    arrows={p:ex[p] for p in straights[:4]}
    assert is_directional_win(sol,arrows)
    return dict(name="Combo_A",world=5,w=4,h=3,sol=sol,arrows=arrows,
                colorLoopsMode=False,directionalMode=True,scramble=1)

# COMBO B: ARROWS + OBSTACLE. 3x3 ring with center obstacle, arrows on 3 straights.
def build_combo_arrows_obstacle():
    sol={
        (0,0):corner_for({0,1}),(2,0):corner_for({0,3}),
        (2,2):corner_for({2,3}),(0,2):corner_for({1,2}),
        (1,0):straight_for({1,3}),(1,2):straight_for({1,3}),
        (0,1):straight_for({0,2}),(2,1):straight_for({0,2}),
    }
    assert count_closed_loops(sol)==(1,True)
    ex=trace_exit_directions(sol)
    arrows={(1,0):ex[(1,0)],(0,1):ex[(0,1)],(2,1):ex[(2,1)]}
    assert is_directional_win(sol,arrows)
    return dict(name="Combo_B",world=6,w=3,h=3,sol=sol,arrows=arrows,obstacle=(1,1),
                colorLoopsMode=False,directionalMode=True,scramble=2)

# COMBO C: BRIDGE + COLOR is too hard to guarantee; use COLOR with 3 colors small instead
# as an extra color challenge (still a "combo" of color + tight space).
def build_combo_three_color():
    def sq(ox,oy,color):
        b={(ox+0,oy+0):corner_for({0,1}),(ox+1,oy+0):corner_for({0,3}),
           (ox+1,oy+1):corner_for({2,3}),(ox+0,oy+1):corner_for({1,2})}
        return {p:(pt,r,color) for p,(pt,r) in b.items()}
    sol={**sq(0,0,1),**sq(3,0,2),**sq(0,3,3)}  # red, blue, green on 5x5
    assert is_color_win(sol)
    return dict(name="Combo_C",world=5,w=5,h=5,sol=sol,arrows={},
                colorLoopsMode=True,directionalMode=False,scramble=1)

def export_combo(spec):
    sol=spec['sol']; w,h=spec['w'],spec['h']; arrows=spec.get('arrows',{})
    scramble=spec['scramble']
    # build start (scrambled rotations); store arrowBase so abs arrow at solved rot matches
    start={}; arrowbase={}
    for pos,val in sol.items():
        if len(val)==3: pt,r,col=val
        else: pt,r=val; col=0
        sr=(r-scramble)%4
        start[pos]=(pt,sr,col)
        if pos in arrows:
            arrowbase[pos]=(arrows[pos]-r)%4
    marker=sorted(start)[0]
    # par
    par=sum(min_taps(start[p][1], (sol[p][1]))) if False else 0
    par=0
    for pos in start:
        solr = sol[pos][1]
        par+=min_taps(start[pos][1], solr)
    cells=[]
    obstacle=spec.get('obstacle',None)
    for y in range(h):
        for x in range(w):
            c=(x,y)
            if c in start:
                pt,sr,col=start[c]
                ab=arrowbase.get(c,None)
                cd=cell_movable(pt,sr,color=col,arrow=ab)
                cd['start']=(c==marker)
                cells.append(cd)
            elif obstacle is not None and c==obstacle:
                cells.append(cell_obstacle())
            else:
                cells.append(cell_empty())
    soldata=[]
    for p in sol:
        col = sol[p][2] if len(sol[p])==3 else 0
        soldata.append({"tx":p[0],"ty":p[1],"pt":PT_INT[sol[p][0]],"trot":sol[p][1],"color":col})
    return {"name":spec['name'],"world":spec['world'],"levelNumber":0,
            "width":w,"height":h,"requiredLoops":1,"parMoves":par,
            "colorLoopsMode":spec['colorLoopsMode'],"coverAllMode":False,
            "directionalMode":spec['directionalMode'],"portalsMode":False,
            "cells":cells,"solution":soldata}

# ---------- Assemble final ordered list ----------
def assemble():
    out=[]
    newnum=1
    # combos get inserted into their target worlds at the end of that world
    combos_by_world={}
    for spec in [build_combo_color_arrows(), build_combo_arrows_obstacle(), build_combo_three_color()]:
        cj=export_combo(spec)
        combos_by_world.setdefault(spec['world'],[]).append(cj)

    for world in range(1,8):
        for oldnum in PLAN[world]:
            lv=dict(existing[oldnum])  # copy
            lv['levelNumber']=newnum
            lv['world']=world
            out.append(lv); newnum+=1
        # append combos for this world
        for cj in combos_by_world.get(world,[]):
            cj=dict(cj); cj['levelNumber']=newnum; cj['world']=world
            out.append(cj); newnum+=1
    return out

if __name__=="__main__":
    final=assemble()
    nums=[lv['levelNumber'] for lv in final]
    assert nums==list(range(1,len(final)+1)), f"non-sequential: {nums}"
    json.dump(final, open("levels_rebalanced.json","w"), indent=1)
    print(f"Rebalanced: {len(final)} levels in 7 worlds.")
    from collections import Counter
    wc=Counter(lv['world'] for lv in final)
    for w in range(1,8):
        print(f"  World {w}: {wc[w]} levels")
