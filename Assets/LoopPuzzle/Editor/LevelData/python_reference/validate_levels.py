"""
Validates all levels. PROVES solvability:
For each level we KNOW the intended solution (constructed). We verify:
  1. solution satisfies win (required_loops closed loops, no open ends)
  2. start does NOT satisfy win (level isn't pre-solved)
  3. multiset of piece TYPES in start == in solution (drag preserves pieces, rotate preserves type)
  4. each displaced start cell's piece type == the solution piece type at its target cell
  5. fixed cells: piece already at solution rotation AND solution piece type matches
  6. all start/solution cells in-bounds, no piece on an obstacle, solution cells distinct
  7. start cells all in-bounds and not on obstacles
  8. compute parMoves = sum over pieces of min_taps(start_rot->sol_rot) + 1 per displaced piece
     (achievable lower bound given +90 tap and one drag per displaced piece)
Because the solution is reachable from start by: dragging each displaced piece to its
target cell (target is empty in start) and rotating each non-fixed piece to its solution
rotation, solvability is GUARANTEED. We additionally re-verify the reconstructed solution.
"""

from level_engine import rot, count_closed_loops, is_win, min_taps
from levels_def import LEVELS
import collections

def piece_types(layout):
    return collections.Counter(pt for pt,_ in layout.values())

def reconstruct_solution_from_start(level):
    """Apply intended moves: place displaced pieces at targets, set every non-fixed
    piece to its solution rotation, keep fixed as-is. Return reconstructed placement."""
    sol = level['solution']
    start = level['start']
    fixed = level['fixed']
    displaced = level['displaced']  # start_cell -> target_cell

    # Map: where does each start piece end up?
    placement = {}
    for cell,(pt,r) in start.items():
        if cell in displaced:
            target = displaced[cell]
            placement[target] = (pt, sol[target][1])  # move + rotate to solution rot
        else:
            # stays in place; rotate to solution rotation (fixed already correct)
            placement[cell] = (pt, sol[cell][1])
    return placement

def validate(level):
    errs=[]
    w,h=level['w'],level['h']
    sol=level['solution']; start=level['start']
    fixed=level['fixed']; obs=level['obstacles']; req=level['required_loops']
    displaced=level['displaced']

    def inb(p): return 0<=p[0]<w and 0<=p[1]<h

    # 1. solution wins
    if not is_win(sol, req):
        n,ok=count_closed_loops(sol)
        errs.append(f"SOLUTION not a win (loops={n}, no_open_ends={ok}, need {req})")

    # 2. start is not already a win
    if is_win(start, req):
        errs.append("START is already solved")

    # 3. piece type multiset preserved
    if piece_types(start)!=piece_types(sol):
        errs.append(f"piece types differ start={dict(piece_types(start))} sol={dict(piece_types(sol))}")

    # 4. displaced types match targets
    for scell,target in displaced.items():
        if scell not in start: errs.append(f"displaced source {scell} not in start"); continue
        if target not in sol: errs.append(f"displaced target {target} not in solution"); continue
        if start[scell][0]!=sol[target][0]:
            errs.append(f"displaced type mismatch {scell}->{target}: {start[scell][0]} vs {sol[target][0]}")
        if target in start:
            errs.append(f"displaced target {target} is occupied in start (must be empty)")

    # 5. fixed pieces already at solution rotation and correct type
    for fcell in fixed:
        if fcell not in start: errs.append(f"fixed {fcell} not in start"); continue
        if fcell not in sol: errs.append(f"fixed {fcell} not in solution"); continue
        if start[fcell]!=sol[fcell]:
            errs.append(f"fixed {fcell} not at solution state: start={start[fcell]} sol={sol[fcell]}")

    # 6/7. bounds + obstacle overlaps + distinct
    for p in sol:
        if not inb(p): errs.append(f"solution cell {p} out of bounds {w}x{h}")
        if p in obs: errs.append(f"solution cell {p} on obstacle")
    for p in start:
        if not inb(p): errs.append(f"start cell {p} out of bounds {w}x{h}")
        if p in obs: errs.append(f"start cell {p} on obstacle")

    # 8. reconstructed solution from start equals a winning config
    recon = reconstruct_solution_from_start(level)
    if not is_win(recon, req):
        n,ok=count_closed_loops(recon)
        errs.append(f"RECONSTRUCTED solution from start is not a win (loops={n}, ok={ok})")
    # reconstructed must occupy exactly the solution cells
    if set(recon.keys())!=set(sol.keys()):
        errs.append(f"reconstructed cells {sorted(recon.keys())} != solution cells {sorted(sol.keys())}")

    # compute parMoves
    par=0
    for cell,(pt,r) in start.items():
        if cell in displaced:
            target=displaced[cell]
            par += 1  # one drag
            par += min_taps(r, sol[target][1])
        elif cell in fixed:
            par += 0
        else:
            par += min_taps(r, sol[cell][1])
    level['parMoves']=par

    return errs

def main():
    print(f"Validating {len(LEVELS)} levels...\n")
    all_ok=True
    # uniqueness check
    seen_solutions=[]
    for lv in LEVELS:
        errs=validate(lv)
        # uniqueness: compare solution footprint+shape
        sig=(lv['w'],lv['h'],tuple(sorted(lv['solution'].keys())),
             tuple(sorted((p,v) for p,v in lv['solution'].items())))
        dup = sig in seen_solutions
        seen_solutions.append(sig)
        status = "OK " if not errs and not dup else "FAIL"
        if errs or dup: all_ok=False
        extra = f"par={lv['parMoves']}" 
        dupmsg = "  <DUPLICATE SOLUTION>" if dup else ""
        print(f"[{status}] {lv['name']} W{lv['world']} {lv['w']}x{lv['h']} "
              f"pieces={len(lv['solution'])} loops={lv['required_loops']} {extra}{dupmsg}")
        for e in errs:
            print(f"        - {e}")
    print()
    # progression sanity: piece counts and board sizes generally non-decreasing per world
    print("Progression (pieces / board area):")
    for lv in LEVELS:
        print(f"  {lv['name']}: {len(lv['solution'])} pieces, {lv['w']*lv['h']} cells, par {lv['parMoves']}")
    print("\nRESULT:", "ALL LEVELS VALID & UNIQUE" if all_ok else "SOME LEVELS FAILED")
    return all_ok

if __name__=="__main__":
    import sys
    ok=main()
    sys.exit(0 if ok else 1)
