"""All 25 levels - distinct shapes, explicit progression, scramble guarantees unsolved start."""
from shapes import loop_from_cycle, rect_cycle
from level_engine import rot, is_win, count_closed_loops, min_taps

LEVELS=[]

def add(name, world, w, h, solution, start, fixed=None, obstacles=None, required_loops=1, displaced=None):
    LEVELS.append(dict(name=name, world=world, w=w, h=h, solution=solution,
                       start=dict(start), fixed=fixed or set(), obstacles=obstacles or set(),
                       required_loops=required_loops, displaced=displaced or {}))

def scramble(solution, fixed=None, offset=1):
    """Rotate every non-fixed piece by `offset` (odd -> always changes a Corner; for a
    Straight, +1/+3 flips its two connectors to the orthogonal pair, a real change)."""
    fixed=fixed or set()
    out={}
    for p,(pt,r) in solution.items():
        out[p]=(pt,r) if p in fixed else (pt,(r+offset)%4)
    return out

# ============ helper: distinct non-rectangular cycles ============
def L_cycle(ox,oy):
    # 8-cell L / staircase hexagon
    return [(ox,oy),(ox+1,oy),(ox+2,oy),(ox+2,oy+1),(ox+1,oy+1),(ox+1,oy+2),(ox,oy+2),(ox,oy+1)]

def U_cycle(ox,oy):
    # 12-cell U / comb shape
    return [(ox,oy),(ox+1,oy),(ox+2,oy),(ox+3,oy),(ox+3,oy+1),(ox+3,oy+2),
            (ox+2,oy+2),(ox+2,oy+1),(ox+1,oy+1),(ox+1,oy+2),(ox,oy+2),(ox,oy+1)]

def T_cycle(ox,oy):
    # 12-cell proper T: horizontal top bar + vertical stem (distinct from plus)
    return [(ox,oy+2),(ox,oy+3),(ox+1,oy+3),(ox+2,oy+3),(ox+3,oy+3),(ox+3,oy+2),
            (ox+2,oy+2),(ox+2,oy+1),(ox+2,oy),(ox+1,oy),(ox+1,oy+1),(ox,oy+1)]

def Z_cycle(ox,oy):
    # 10-cell Z / step
    return [(ox,oy),(ox+1,oy),(ox+1,oy+1),(ox+2,oy+1),(ox+2,oy+2),(ox+3,oy+2),
            (ox+3,oy+1)] if False else \
           [(ox,oy),(ox+1,oy),(ox+1,oy+1),(ox+2,oy+1),(ox+2,oy+2),(ox+1,oy+2),(ox+1,oy+1)]

def plus_cycle(ox,oy):
    # 12-cell plus / cross outline
    return [(ox+1,oy),(ox+2,oy),(ox+2,oy+1),(ox+3,oy+1),(ox+3,oy+2),(ox+2,oy+2),
            (ox+2,oy+3),(ox+1,oy+3),(ox+1,oy+2),(ox,oy+2),(ox,oy+1),(ox+1,oy+1)]

def bigL_cycle(ox,oy):
    # 10-cell tall L
    return [(ox,oy),(ox+1,oy),(ox+2,oy),(ox+2,oy+1),(ox+1,oy+1),(ox+1,oy+2),
            (ox+1,oy+3),(ox,oy+3),(ox,oy+2),(ox,oy+1)]

# =================================================================
# WORLD 1 — Learning (rotate only). Distinct shapes & growing size.
# =================================================================
# L1: smallest 2x2 square (4 cells)
s=loop_from_cycle(rect_cycle(1,1,2,2)); add("Level_01",1,4,4,s,scramble(s,offset=2))
# L2: 2x3 tall rectangle (6 cells)
s=loop_from_cycle(rect_cycle(1,1,2,3)); add("Level_02",1,4,5,s,scramble(s,offset=1))
# L3: 3x2 wide rectangle (6 cells) - different orientation
s=loop_from_cycle(rect_cycle(1,1,3,2)); add("Level_03",1,5,4,s,scramble(s,offset=3))
# L4: 3x3 square (8 cells)
s=loop_from_cycle(rect_cycle(1,1,3,3)); add("Level_04",1,5,5,s,scramble(s,offset=1))
# L5: L-shape (8 cells) - first non-rectangular, visually distinct
s=loop_from_cycle(L_cycle(1,1)); add("Level_05",1,5,5,s,scramble(s,offset=2))

# =================================================================
# WORLD 2 — Obstacles (drag + rotate). Displaced pieces + obstacle cells.
# =================================================================
def displace(solution, moves, w, h, fixed=None, offset=1):
    """moves: dict target_cell -> park_cell. Returns (start, displaced_map)."""
    start=scramble(solution, fixed=fixed, offset=offset)
    dmap={}
    for target,park in moves.items():
        pt,_=start.pop(target)
        start[park]=(pt,0)
        dmap[park]=target
    return start, dmap

# L6: 2x2 square, 1 displaced, 1 obstacle
s=loop_from_cycle(rect_cycle(1,1,2,2))
st,dm=displace(s,{(2,2):(4,0)},5,5,offset=1); add("Level_06",2,5,5,s,st,obstacles={(0,4)},displaced=dm)
# L7: 3x2 rectangle, 1 displaced, 2 obstacles
s=loop_from_cycle(rect_cycle(1,1,3,2))
st,dm=displace(s,{(3,2):(0,4)},5,5,offset=2); add("Level_07",2,5,5,s,st,obstacles={(4,4),(0,0)},displaced=dm)
# L8: L-shape, 2 displaced, obstacles
s=loop_from_cycle(L_cycle(1,1))
st,dm=displace(s,{(3,1):(5,0),(1,3):(0,5)},6,6,offset=3); add("Level_08",2,6,6,s,st,obstacles={(5,5)},displaced=dm)
# L9: 3x4 rectangle, 1 displaced, obstacle wall
s=loop_from_cycle(rect_cycle(1,1,3,4))
st,dm=displace(s,{(2,1):(5,5)},6,6,offset=1); add("Level_09",2,6,6,s,st,obstacles={(5,0),(5,1),(5,2)},displaced=dm)
# L10: 4x4 square, 3 displaced
s=loop_from_cycle(rect_cycle(1,1,4,4))
st,dm=displace(s,{(1,1):(0,0),(4,4):(5,5),(2,4):(0,5)},6,6,offset=2); add("Level_10",2,6,6,s,st,obstacles={(5,0)},displaced=dm)

# =================================================================
# WORLD 3 — Fixed pieces. Some loop cells fixed at correct rotation.
# =================================================================
# L11: U-shape (12 cells), 2 fixed
s=loop_from_cycle(U_cycle(1,1)); fx={(1,1),(4,1)}; add("Level_11",3,6,5,s,scramble(s,fixed=fx,offset=2),fixed=fx)
# L12: plus/cross shape (12 cells), 3 fixed - distinct shape
s=loop_from_cycle(plus_cycle(1,1)); fx={(2,1),(2,4),(4,2)}; add("Level_12",3,6,6,s,scramble(s,fixed=fx,offset=1),fixed=fx)
# L13: T-shape (12 cells), 2 fixed - distinct shape
s=loop_from_cycle(T_cycle(1,1)); fx={(2,1),(3,4)}; add("Level_13",3,6,6,s,scramble(s,fixed=fx,offset=3),fixed=fx)
# L14: big-L shape (10 cells), 2 fixed + 1 displaced + obstacle - distinct shape
s=loop_from_cycle(bigL_cycle(1,1)); fx={(1,1),(3,2)}
st,dm=displace(s,{(3,1):(0,5)},6,6,fixed=fx,offset=3); add("Level_14",3,6,6,s,st,fixed=fx,obstacles={(5,0)},displaced=dm)
# L15: 4x5 rectangle, 3 fixed, 2 displaced
s=loop_from_cycle(rect_cycle(1,1,4,5)); fx={(1,1),(4,1),(1,5)}
st,dm=displace(s,{(4,3):(0,0),(2,5):(5,6)},6,7,fixed=fx,offset=1); add("Level_15",3,6,7,s,st,fixed=fx,obstacles={(5,0),(5,1)},displaced=dm)

# =================================================================
# WORLD 4 — Multiple loops (require 2 closed loops).
# =================================================================
def two_loops(c1,c2):
    s=dict(loop_from_cycle(c1)); s.update(loop_from_cycle(c2)); return s
# L16: two 2x2 squares
s=two_loops(rect_cycle(0,0,1,1),rect_cycle(3,3,4,4)); add("Level_16",4,6,6,s,scramble(s,offset=2),required_loops=2)
# L17: 2x2 + 2x3
s=two_loops(rect_cycle(0,0,1,1),rect_cycle(3,2,4,4)); add("Level_17",4,6,6,s,scramble(s,offset=1),required_loops=2)
# L18: two 2x2 + displaced
s=two_loops(rect_cycle(0,0,1,1),rect_cycle(4,4,5,5))
st,dm=displace(s,{(1,1):(0,3),(4,4):(3,0)},6,6,offset=3); add("Level_18",4,6,6,s,st,required_loops=2,displaced=dm)
# L19: 2x3 + 3x2 with obstacles
s=two_loops(rect_cycle(0,0,1,2),rect_cycle(4,4,6,5)); add("Level_19",4,7,7,s,scramble(s,offset=2),required_loops=2,obstacles={(6,0),(0,6)})
# L20: 3x3 + 2x2, fixed + displaced
s=two_loops(rect_cycle(0,0,2,2),rect_cycle(4,4,5,5)); fx={(0,0)}
st,dm=displace(s,{(5,5):(0,6)},7,7,fixed=fx,offset=1); add("Level_20",4,7,7,s,st,required_loops=2,fixed=fx,displaced=dm)

# =================================================================
# WORLD 5 — Advanced. Triple/Cross (figure-8) + large complex loops.
# =================================================================
# Figure-8 with two Triples (one connected component, topologically two loops)
from itertools import product as _prod
def solve_fig8(ox,oy):
    cells={
     (ox,oy):"Corner",(ox+2,oy):"Corner",(ox,oy+2):"Corner",(ox+2,oy+2):"Corner",
     (ox+1,oy):"Straight",(ox+1,oy+2):"Straight",(ox+1,oy+1):"Straight",
     (ox,oy+1):"Triple",(ox+2,oy+1):"Triple",
    }
    keys=list(cells)
    for combo in _prod(range(4),repeat=len(keys)):
        pl={keys[i]:(cells[keys[i]],combo[i]) for i in range(len(keys))}
        n,ok=count_closed_loops(pl)
        if ok and n==1:
            # require both triples be degree-3 (true junctions)
            if all(sum(rot(*pl[k]))==3 for k in [(ox,oy+1),(ox+2,oy+1)]):
                return pl
    return None

# L21: 5x5 big perimeter (16 cells) - pure scale step up
s=loop_from_cycle(rect_cycle(1,1,5,5)); add("Level_21",5,7,7,s,scramble(s,offset=2))
# L22: 5x4 rectangle, 2 displaced + obstacles
s=loop_from_cycle(rect_cycle(1,1,5,4))
st,dm=displace(s,{(3,1):(0,6),(3,4):(6,0)},7,7,offset=1); add("Level_22",5,7,7,s,st,obstacles={(0,0),(6,6)},displaced=dm)
# L23: FIGURE-8 with triples (distinct mechanic showcase)
f8=solve_fig8(1,1); assert f8, "fig8 unsolved"; add("Level_23",5,5,5,f8,scramble(f8,offset=1))
# L24: 5x6 large loop, 3 displaced + obstacle
s=loop_from_cycle(rect_cycle(1,1,5,6))
st,dm=displace(s,{(3,1):(0,0),(1,4):(6,7),(5,3):(6,0)},7,8,offset=2); add("Level_24",5,7,8,s,st,obstacles={(0,7)},displaced=dm)
# L25: FINALE 6x6 perimeter (20 cells), fixed + displaced + obstacles
s=loop_from_cycle(rect_cycle(1,1,6,6)); fx={(1,1),(6,6)}
st,dm=displace(s,{(3,1):(0,0),(4,6):(7,7)},8,8,fixed=fx,offset=1); add("Level_25",5,8,8,s,st,fixed=fx,obstacles={(7,0),(0,7)},displaced=dm)

if __name__=="__main__":
    print(f"Defined {len(LEVELS)} levels")
