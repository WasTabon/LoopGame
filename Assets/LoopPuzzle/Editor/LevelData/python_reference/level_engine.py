"""
Loop Puzzle - level design + validation engine.
Mirrors the in-game model EXACTLY:
  - Directions N=0,E=1,S=2,W=3 ; offsets +Y north
  - rot(steps): dir d -> (d+steps)%4
  - win: at least `required_loops` closed loops; every piece touched by the
    component(s) has all connectors mutual (no open ends), degree>=2.

Extended (iteration 11) for new mechanics:
  - Bridge piece: all 4 connectors but two INDEPENDENT channels (N-S and E-W),
    so flow passes straight through without merging.
  - Colors: pieces may carry a color; two adjacent pieces link only if colors
    are compatible (same color, or either is neutral=0).
Backward compatible: with neutral colors and no bridges, behaves exactly as before.
"""

from itertools import product

OFF = {0:(0,1),1:(1,0),2:(0,-1),3:(-1,0)}
OPP = {0:2,1:3,2:0,3:1}

def base_conn(pt):
    c=[False]*4
    if pt=="Straight": c[0]=c[2]=True
    elif pt=="Corner": c[0]=c[1]=True
    elif pt=="Triple": c[0]=c[1]=c[3]=True
    elif pt=="Cross": c[0]=c[1]=c[2]=c[3]=True
    elif pt=="Bridge": c[0]=c[1]=c[2]=c[3]=True
    return tuple(c)

def rot(pt,s):
    b=base_conn(pt); r=[False]*4
    for d in range(4): r[(d+s)%4]=b[d]
    return tuple(r)

def colors_compatible(a, b):
    if a==0 or b==0: return True
    return a==b

def _normalize(placement):
    """Accepts placement values as (pt,rot) or (pt,rot,color). Returns dict
    pos -> (pt, rot, color)."""
    out={}
    for pos,val in placement.items():
        if len(val)==3:
            out[pos]=val
        else:
            pt,r=val
            out[pos]=(pt,r,0)
    return out

def count_closed_loops(placement):
    """placement: dict (x,y)->(pt,rot) or (pt,rot,color).
    Returns (num_loops, ok_no_open_ends).
    Valid if every piece is part of a closed loop: all connectors mutual
    (color-compatible), every piece degree>=2. num_loops = connected components,
    where bridge channels keep N-S and E-W traversal independent."""
    pl=_normalize(placement)
    conn={pos:rot(pt,r) for pos,(pt,r,col) in pl.items()}
    color={pos:col for pos,(pt,r,col) in pl.items()}
    ptype={pos:pt for pos,(pt,r,col) in pl.items()}

    # check no open ends anywhere (a connector must face a compatible mutual connector)
    for (x,y),c in conn.items():
        deg=0
        for d in range(4):
            if c[d]:
                deg+=1
                dx,dy=OFF[d]; n=(x+dx,y+dy)
                if n not in conn or not conn[n][OPP[d]]:
                    return 0, False
                if not colors_compatible(color[(x,y)], color[n]):
                    return 0, False
        if deg<2:
            return 0, False

    # Count loops by traversing channels. For bridges, entering from d exits OPP[d]
    # only; for normal pieces, a component is the usual connected set.
    # We trace edge-following to separate bridge channels.
    # Build half-edges: (cell, dir). Walk: from (cell,d_out) step to neighbor,
    # enter from opposite, pick exit via channel rule.
    visited_edges=set()
    loops=0

    def exit_dirs(cell, enter):
        pt=ptype[cell]; c=conn[cell]
        if pt=="Bridge":
            o=OPP[enter]
            return [o] if c[o] else []
        return [d for d in range(4) if c[d] and d!=enter]

    # Each closed loop is a cycle of half-edges. Count distinct cycles.
    all_edges=[]
    for cell in conn:
        for d in range(4):
            if conn[cell][d]:
                all_edges.append((cell,d))

    for start_edge in all_edges:
        if start_edge in visited_edges: continue
        # follow the cycle starting by leaving 'cell' via 'd'
        cell,d=start_edge
        cycle=[]
        ok=True
        cur_cell,cur_out=cell,d
        guard=0
        while True:
            guard+=1
            if guard>10000: ok=False; break
            if (cur_cell,cur_out) in visited_edges:
                break
            visited_edges.add((cur_cell,cur_out))
            cycle.append((cur_cell,cur_out))
            dx,dy=OFF[cur_out]; nxt=(cur_cell[0]+dx,cur_cell[1]+dy)
            enter=OPP[cur_out]
            outs=exit_dirs(nxt,enter)
            if len(outs)!=1:
                # junction (Triple/Cross) -> not a simple single cycle through here;
                # fall back: treat as connected-component counting for safety
                ok=False; break
            cur_cell=nxt; cur_out=outs[0]
            if (cur_cell,cur_out)==start_edge:
                break
        if ok and len(cycle)>0:
            loops+=1

    # If any junction made edge-following ambiguous, fall back to component count
    # (this matches the in-game ValidateAllLoops connected-component semantics).
    # Detect junctions:
    has_junction=any(ptype[cell] in ("Triple","Cross") for cell in conn)
    if has_junction:
        return _component_count(conn,color), True

    # edge-following counts each undirected loop twice (once per direction)
    return loops//2 if loops>0 else _component_count(conn,color), True

def _component_count(conn, color):
    seen=set(); comps=0
    for start in conn:
        if start in seen: continue
        comps+=1
        stack=[start]
        while stack:
            cur=stack.pop()
            if cur in seen: continue
            seen.add(cur)
            c=conn[cur]
            for d in range(4):
                if c[d]:
                    dx,dy=OFF[d]; n=(cur[0]+dx,cur[1]+dy)
                    if n in conn and n not in seen and colors_compatible(color[cur],color[n]):
                        stack.append(n)
    return comps

def is_win(placement, required_loops):
    n, ok = count_closed_loops(placement)
    return ok and n>=required_loops

def is_color_win(placement):
    """Every color present must have at least one closed loop of that color."""
    pl=_normalize(placement)
    n, ok = count_closed_loops(pl)
    if not ok or n==0: return False
    colors_present={col for (pt,r,col) in pl.values() if col!=0}
    if not colors_present:
        return n>=1
    # which colors appear in valid loops? since count_closed_loops already requires
    # ALL pieces be in closed loops, every present color is looped.
    return True

def min_taps(start_rot, target_rot):
    return (target_rot - start_rot) % 4


def trace_exit_directions(placement):
    """Trace a single simple loop, returning {pos: exit_dir}. None if junction/open.
    placement: pos->(pt,rot) or (pt,rot,color)."""
    pl=_normalize(placement)
    conn={pos:rot(pt,r) for pos,(pt,r,col) in pl.items()}
    start=next(iter(conn))
    c=conn[start]
    first=next((d for d in range(4) if c[d]), None)
    if first is None: return None
    cell=start; cur=first; out={}; guard=0
    while guard<10000:
        guard+=1
        out[cell]=cur
        dx,dy=OFF[cur]; nxt=(cell[0]+dx,cell[1]+dy)
        if nxt not in conn: return None
        enter=OPP[cur]; nc=conn[nxt]
        exits=[d for d in range(4) if nc[d] and d!=enter]
        if len(exits)!=1: return None
        cell=nxt; cur=exits[0]
        if cell==start and cur==first: break
    return out

def is_directional_win(placement, arrows):
    """arrows: pos->absolute_arrow_dir (already rotated) for directional pieces.
    Win if loop closed and an orientation satisfies all arrows."""
    n,ok=count_closed_loops(placement)
    if not ok or n==0: return False
    exit_of=trace_exit_directions(placement)
    if exit_of is None:
        return False
    a1=all(arrows[c]==exit_of[c] for c in arrows if arrows[c] is not None)
    a2=all(arrows[c]==OPP[exit_of[c]] for c in arrows if arrows[c] is not None)
    return a1 or a2
