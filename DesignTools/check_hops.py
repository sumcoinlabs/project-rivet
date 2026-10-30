#!/usr/bin/env python3
"""Approximate default hop displacement with Unity 3D drag; guide only, not a Unity playtest."""
from math import sin,cos,radians,exp
v=5.88
angle=radians(58)
vx=v*cos(angle)
vy=v*sin(angle)
dt=0.0005
for rise in [0.72,0.68,0.48]:
 x=y=t=0.
 ax,ay=vx,vy
 while t<2:
  ax*=exp(-.14*dt)
  ay+=(-9.81-.14*ay)*dt
  x+=ax*dt
  y+=ay*dt
  t+=dt
  if t>.2 and ay<0 and y<=rise: break
 print(f"rise {rise:0.2f}, hop {x:0.2f} units, time {t:0.2f}s")
