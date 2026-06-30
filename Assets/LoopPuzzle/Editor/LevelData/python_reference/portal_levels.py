"""Iteration 16 demo levels (38-40): Portals. Portal pairs teleport the loop across
the board. Validated: solution forms a closed loop through portals, start unsolved,
reconstruction wins.
Note: portals are FIXED cells (not rotatable). The player rotates the normal pieces;
portal mouths are fixed by the level design."""
import json
from level_engine import is_portal_win, rot as ROT, min_taps

PT_INT={"None":0,"Straight":1,"Corner":2,"Triple":3,"Cross":4,"Bridge":5}
CT_INT={"Empty":0,"Fixed":1,"Movable":2,"Obstacle":3,"Start":4,"Portal":5,"Breakable":6}

def straight_for(dirs):
    dirs=frozenset(dirs)
    for r in range(4):
        if frozenset(d for d in range(4) if ROT("Straight",r)[d])==dirs: return ("Straight",r)
    raise ValueError(dirs)
def corner_for(dirs):
    dirs=frozenset(dirs)
    for r in range(4):
        if frozenset(d for d in range(4) if ROT("Corner",r)[d])==dirs: return ("Corner",r)
    raise ValueError(dirs)

LEVELS=[]

def add_level(name, world, w, h, pieces, portals, scramble=1):
    assert is_portal_win(pieces, portals), f"{name}: solution not portal-win"
    start={}
    for pos,(pt,r) in pieces.items():
        start[pos]=(pt,(r-scramble)%4)
    assert not is_portal_win({p:start[p] for p in start}, portals), f"{name}: start solved"
    LEVELS.append(dict(name=name,world=world,w=w,h=h,pieces=pieces,start=start,
                       portals=portals,scramble=scramble))

# L38: two vertical segments (cols 0 and 2), joined top+bottom by 2 portal pairs.
# Board 3x3 grid area; portals sit just outside the segment ends within the board.
# Use a 3-wide x 4-tall board: segments at x=0 and x=2, rows 1-2; portals at rows 0 and 3.
# Segment L: (0,1)(0,2) vertical straights. Segment R: (2,1)(2,2).
# Top portals: (0,3) mouth=S faces (0,2); (2,3) mouth=S faces (2,2). id=1
# Bottom portals: (0,0) mouth=N faces (0,1); (2,0) mouth=N faces (2,1). id=2
pieces={
 (0,1):straight_for({0,2}),(0,2):straight_for({0,2}),
 (2,1):straight_for({0,2}),(2,2):straight_for({0,2}),
}
portals={(0,3):(1,2),(2,3):(1,2),(0,0):(2,0),(2,0):(2,0)}
add_level("Level_38",5,3,4,pieces,portals,scramble=1)

# L39: a path with a single portal pair that "wraps" one side.
# L-shaped path on left + portal jump to a piece on the right.
# Board 4x4. Left loop partial: (0,0)(0,1)(1,1)(1,0) would be a 2x2, but we break it
# and route through portals. Keep it a clean two-segment again but wider.
# Segments at x=0 (rows1-2) and x=3 (rows1-2); portals rows 0 and 3.
pieces={
 (0,1):straight_for({0,2}),(0,2):straight_for({0,2}),
 (3,1):straight_for({0,2}),(3,2):straight_for({0,2}),
}
portals={(0,3):(1,2),(3,3):(1,2),(0,0):(2,0),(3,0):(2,0)}
add_level("Level_39",5,4,4,pieces,portals,scramble=1)

# L40: three-tall segments joined by portals (more pieces).
pieces={
 (0,1):straight_for({0,2}),(0,2):straight_for({0,2}),(0,3):straight_for({0,2}),
 (2,1):straight_for({0,2}),(2,2):straight_for({0,2}),(2,3):straight_for({0,2}),
}
portals={(0,4):(1,2),(2,4):(1,2),(0,0):(2,0),(2,0):(2,0)}
add_level("Level_40",5,3,5,pieces,portals,scramble=1)

def validate(lv):
    errs=[]
    pieces=lv['pieces']; start=lv['start']; portals=lv['portals']
    if not is_portal_win(pieces, portals): errs.append("solution not portal-win")
    if is_portal_win({p:start[p] for p in start}, portals): errs.append("start solved")
    recon={pos:(start[pos][0], pieces[pos][1]) for pos in start}
    if not is_portal_win(recon, portals): errs.append("reconstruction fails")
    for p in list(pieces)+list(portals):
        if not (0<=p[0]<lv['w'] and 0<=p[1]<lv['h']): errs.append(f"OOB {p}")
    return errs

def export():
    out=[]
    for lv in LEVELS:
        errs=validate(lv)
        if errs:
            print(f"FAIL {lv['name']}: {errs}"); return None
        w,h=lv['w'],lv['h']; pieces=lv['pieces']; start=lv['start']; portals=lv['portals']
        marker=sorted(start)[0]
        par=sum(min_taps(start[p][1], pieces[p][1]) for p in start)
        cells=[]
        for y in range(h):
            for x in range(w):
                c=(x,y)
                if c in start:
                    pt,r=start[c]
                    cells.append({"ct":CT_INT["Movable"],"pt":PT_INT[pt],"rot":r,
                                  "start":(c==marker),"color":0,"portalId":0,"portalDir":0,
                                  "maxRotations":0,"directional":False,"arrowDir":0})
                elif c in portals:
                    pid,mdir=portals[c]
                    cells.append({"ct":CT_INT["Portal"],"pt":0,"rot":0,"start":False,
                                  "color":0,"portalId":pid,"portalDir":mdir,
                                  "maxRotations":0,"directional":False,"arrowDir":0})
                else:
                    cells.append({"ct":CT_INT["Empty"],"pt":0,"rot":0,"start":False,
                                  "color":0,"portalId":0,"portalDir":0,
                                  "maxRotations":0,"directional":False,"arrowDir":0})
        soldata=[{"tx":p[0],"ty":p[1],"pt":PT_INT[pieces[p][0]],"trot":pieces[p][1],"color":0}
                 for p in pieces]
        out.append({"name":lv['name'],"world":lv['world'],
                    "levelNumber":int(lv['name'].split('_')[1]),
                    "width":w,"height":h,"requiredLoops":1,"parMoves":par,
                    "colorLoopsMode":False,"coverAllMode":False,"directionalMode":False,
                    "portalsMode":True,"cells":cells,"solution":soldata})
    return out

if __name__=="__main__":
    data=export()
    if data:
        json.dump(data, open("portal_levels_export.json","w"), indent=1)
        print(f"Exported {len(data)} portal levels.")
        for lv in data:
            np=sum(1 for c in lv['cells'] if c['ct']==CT_INT['Portal'])
            print(f"  {lv['name']}: {lv['width']}x{lv['height']}, par {lv['parMoves']}, {np} portal cells")
