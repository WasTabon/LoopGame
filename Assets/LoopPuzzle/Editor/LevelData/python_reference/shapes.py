"""Loop shape library: ordered cycles -> validated loop placements."""
from level_engine import count_closed_loops, rot, OFF

def _dir_between(a, b):
    dx, dy = b[0]-a[0], b[1]-a[1]
    for d,(ox,oy) in OFF.items():
        if (dx,dy)==(ox,oy): return d
    raise ValueError(f"cells {a},{b} not adjacent")

def _piece_for(conns):
    conns=set(conns)
    for pt in ("Straight","Corner"):
        for r in range(4):
            c=rot(pt,r)
            if {d for d in range(4) if c[d]}==conns:
                return (pt,r)
    raise ValueError(f"no 2-connector piece for {conns}")

def loop_from_cycle(cycle):
    assert len(set(cycle))==len(cycle), f"repeated cell in cycle {cycle}"
    n=len(cycle)
    placement={}
    for i,cell in enumerate(cycle):
        prev=cycle[(i-1)%n]; nxt=cycle[(i+1)%n]
        d_in=_dir_between(cell,prev); d_out=_dir_between(cell,nxt)
        placement[cell]=_piece_for({d_in,d_out})
    nloops,ok=count_closed_loops(placement)
    assert ok and nloops==1, f"invalid loop: loops={nloops} ok={ok}"
    return placement

def rect_cycle(x0,y0,x1,y1):
    """Ordered perimeter cycle of a rectangle (works for any w,h>=2-span)."""
    pts=[]
    for x in range(x0,x1): pts.append((x,y0))
    for y in range(y0,y1): pts.append((x1,y))
    for x in range(x1,x0,-1): pts.append((x,y1))
    for y in range(y1,y0,-1): pts.append((x0,y))
    return pts
