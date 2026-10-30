from pathlib import Path
import cairosvg
root=Path(__file__).resolve().parent.parent / 'Assets/_Rivet/Resources/RivetArt'
root.mkdir(parents=True, exist_ok=True)
def save(name, svg):
    svg=svg.strip().replace('xmlns="http://www.w3.org/2000/svg"','')
    if '<svg ' in svg: svg=svg.replace('<svg ','<svg xmlns="http://www.w3.org/2000/svg" ',1)
    cairosvg.svg2png(bytestring=svg.encode(),write_to=str(root/(name+'.png')))
# High resolution, original authored vector artwork, not a third-party image.
back='''<svg width="1920" height="1080" viewBox="0 0 1920 1080"><defs><linearGradient id="sky" x2="0" y2="1"><stop stop-color="#111b46"/><stop offset=".47" stop-color="#234d76"/><stop offset="1" stop-color="#2c88a0"/></linearGradient><radialGradient id="light"><stop stop-color="#92e0ff" stop-opacity=".7"/><stop offset="1" stop-color="#8de4ef" stop-opacity="0"/></radialGradient><linearGradient id="ground" x2="0" y2="1"><stop stop-color="#193352"/><stop offset="1" stop-color="#0c1730"/></linearGradient><linearGradient id="building" x2="1" y2="0"><stop stop-color="#234362"/><stop offset="1" stop-color="#162c4b"/></linearGradient><filter id="glow"><feGaussianBlur stdDeviation="15"/></filter></defs>
<rect width="1920" height="1080" fill="url(#sky)"/><circle cx="1480" cy="210" r="570" fill="url(#light)"/>
<g opacity=".4" fill="#77bad5"><path d="M0 610L185 495 310 565 455 490 660 610 820 545 1060 630 1200 560 1480 610 1660 515 1920 625V1080H0Z"/></g>
<g fill="#123353" opacity=".83"><path d="M0 680V515h100v-90h95v100h65V385h110v275h85V470h130v190h100V380h95v260h90V480h135v170h105V330h110v310h75V465h100v185h100V410h110v240h100V480h95v180H1920V1080H0Z"/></g>
<g opacity=".62" fill="#77daeb">'''
for x in range(15,1920,66):
 for y in range(470,685,59):
  if ((x*13+y*7)//11)%4!=0: back+=f'<rect x="{x}" y="{y}" width="11" height="18" rx="2" opacity=".33"/>'
back+='''</g><path d="M0 720 C360 665 680 700 930 720S1520 660 1920 705 V1080H0Z" fill="url(#ground)"/>
<path d="M0 765 Q920 733 1920 780" stroke="#5de2e9" stroke-width="8" opacity=".28" fill="none"/>
<g stroke="#53e1f2" stroke-width="2" opacity=".1">'''
for y in range(795,1200,52):back+=f'<path d="M0 {y} H1920"/>'
for x in range(-250,2500,120):back+=f'<path d="M960 724 L{x} 1080"/>'
back+='''</g><g opacity=".24"><circle cx="250" cy="180" r="110" fill="#6ad5e8" filter="url(#glow)"/><circle cx="1670" cy="600" r="115" fill="#a4b3ff" filter="url(#glow)"/></g>
<g fill="#c5eaff" opacity=".28"><rect x="80" y="126" width="170" height="7" rx="3"/><rect x="80" y="144" width="90" height="5" rx="3"/><rect x="1650" y="120" width="155" height="6" rx="3"/></g>
<g opacity=".6"><circle cx="1450" cy="190" r="76" fill="none" stroke="#90e6f7" stroke-width="5"/><circle cx="1450" cy="190" r="93" fill="none" stroke="#90e6f7" stroke-opacity=".28" stroke-width="4"/></g>
</svg>'''
save('garage_backdrop',back)
platform='''<svg width="1000" height="160" viewBox="0 0 1000 160"><defs><linearGradient id="b" x2="0" y2="1"><stop stop-color="#516d93"/><stop offset=".4" stop-color="#1a365a"/><stop offset="1" stop-color="#0d1d37"/></linearGradient><linearGradient id="top" x2="0" y2="1"><stop stop-color="#a9ecf5"/><stop offset=".5" stop-color="#35d7ee"/><stop offset="1" stop-color="#1385bd"/></linearGradient></defs><rect x="4" y="10" width="992" height="145" rx="36" fill="#07172a" opacity=".55"/><rect x="7" y="2" width="986" height="132" rx="29" fill="url(#b)" stroke="#5d789d" stroke-width="6"/><rect x="9" y="0" width="982" height="38" rx="18" fill="url(#top)"/><path d="M25 42H975" stroke="#14365d" stroke-width="5"/><path d="M65 105h870" stroke="#427393" stroke-width="4" opacity=".45"/><g fill="#92c3d5"><circle cx="40" cy="78" r="6"/><circle cx="960" cy="78" r="6"/></g></svg>'''
save('platform',platform)
car='''<svg width="1100" height="500" viewBox="0 0 1100 500"><defs><linearGradient id="body" x2="0" y2="1"><stop stop-color="#fafcff"/><stop offset=".52" stop-color="#e2f0fb"/><stop offset="1" stop-color="#9cb9d1"/></linearGradient><linearGradient id="roof" x2="0" y2="1"><stop stop-color="#4b7698"/><stop offset="1" stop-color="#0b2647"/></linearGradient><linearGradient id="glass" x2=".5" y2="1"><stop stop-color="#a8efff"/><stop offset=".35" stop-color="#3e9bca"/><stop offset="1" stop-color="#152a55"/></linearGradient><filter id="drop"><feGaussianBlur stdDeviation="8"/></filter></defs><ellipse cx="552" cy="422" rx="445" ry="35" fill="#061c34" opacity=".38" filter="url(#drop)"/>
<path d="M150 322 Q162 259 250 249 L330 151 Q371 107 472 107 H666 Q733 107 790 163 L857 251 Q965 253 988 311 L989 366 Q977 397 924 401 H188 Q134 398 135 354Z" fill="url(#body)" stroke="#597a9e" stroke-width="12" stroke-linejoin="round"/>
<path d="M330 241 L400 164 Q424 140 476 139 H649 Q704 141 735 175 L795 243Z" fill="url(#glass)" stroke="#88cfe5" stroke-width="11" stroke-linejoin="round"/>
<path d="M544 139V245" stroke="#b6eaff" stroke-opacity=".7" stroke-width="11"/>
<path d="M250 253H863" stroke="#e4ffff" stroke-width="8" opacity=".65"/>
<path d="M194 325 Q540 343 944 312" fill="none" stroke="#f7ffff" stroke-width="11" opacity=".8"/>
<path d="M196 352H927" stroke="#6eaccb" stroke-width="8" opacity=".55"/>
<rect x="242" y="265" width="158" height="14" rx="7" fill="#7cdbea" opacity=".7"/>
<path d="M876 281 Q953 279 969 307 L959 326 877 316Z" fill="#91faff" stroke="#ddffff" stroke-width="7"/>
<path d="M140 312L224 300 235 325 152 334Z" fill="#79e2f5"/>
<path d="M833 369H911" stroke="#394e66" stroke-width="10" stroke-linecap="round"/>
<path d="M836 370q29 18 55 0" fill="none" stroke="#233d5d" stroke-width="10" stroke-linecap="round"/>
<g fill="#e9fbff"><ellipse cx="696" cy="216" rx="32" ry="34"/><ellipse cx="768" cy="216" rx="31" ry="34"/></g>
<g fill="#132e52"><ellipse cx="704" cy="216" rx="14" ry="22"/><ellipse cx="777" cy="216" rx="14" ry="22"/></g>
<circle cx="708" cy="203" r="6" fill="white"/><circle cx="780" cy="202" r="5" fill="white"/>
<path d="M849 360q35 30 63-3" fill="none" stroke="#1c486c" stroke-width="11" stroke-linecap="round"/>
<path d="M200 382h730" stroke="#1b385f" stroke-width="12" stroke-linecap="round" opacity=".6"/>
<circle cx="305" cy="391" r="86" fill="#0f223a" stroke="#384c69" stroke-width="9"/><circle cx="795" cy="391" r="86" fill="#0f223a" stroke="#384c69" stroke-width="9"/>
<circle cx="305" cy="391" r="50" fill="#96b4d6" stroke="#e3f4ff" stroke-width="7"/><circle cx="795" cy="391" r="50" fill="#96b4d6" stroke="#e3f4ff" stroke-width="7"/>
<g fill="#24466c"><circle cx="305" cy="391" r="19"/><circle cx="795" cy="391" r="19"/></g>
<path d="M210 372Q200 316 253 314L359 314Q410 333 399 382" fill="none" stroke="#2e4d6f" stroke-width="10" opacity=".95"/>
<path d="M700 374Q692 316 755 307L842 314Q907 337 886 382" fill="none" stroke="#2e4d6f" stroke-width="10" opacity=".95"/>
<path d="M380 156L665 156" stroke="#fff" stroke-width="9" stroke-linecap="round" opacity=".85"/>
</svg>'''
save('wingo_car',car)
wing='''<svg width="600" height="200" viewBox="0 0 600 200"><defs><linearGradient id="w" x2="0" y2="1"><stop stop-color="#f2fbff"/><stop offset="1" stop-color="#6eacc8"/></linearGradient></defs><path d="M45 180 L115 44 Q135 15 205 16 H505 Q553 16 573 50 L590 135 547 186Z" fill="url(#w)" stroke="#477c9e" stroke-width="13" stroke-linejoin="round"/><path d="M149 49H484 Q522 49 532 91 L540 141 H100Z" fill="#245779" stroke="#a8e8fb" stroke-width="8"/><path d="M168 58L312 58 192 137 120 137Z" fill="#6cdcf4" opacity=".45"/><path d="M560 165H88" stroke="#d4feff" stroke-width="7"/></svg>'''
save('wing_panel',wing)
charger='''<svg width="500" height="710" viewBox="0 0 500 710"><defs><linearGradient id="cab" x2="0" y2="1"><stop stop-color="#f4fbff"/><stop offset="1" stop-color="#9cbcd2"/></linearGradient><linearGradient id="disp" x2="0" y2="1"><stop stop-color="#0f3d6b"/><stop offset="1" stop-color="#12314e"/></linearGradient></defs><ellipse cx="245" cy="674" rx="190" ry="26" fill="#061b37" opacity=".35"/><rect x="78" y="50" width="340" height="602" rx="79" fill="#102847" stroke="#62daec" stroke-width="15"/><rect x="97" y="68" width="302" height="553" rx="63" fill="url(#cab)"/><rect x="150" y="135" width="197" height="270" rx="30" fill="url(#disp)" stroke="#70e5f0" stroke-width="11"/><path d="M252 158L209 274h47l-18 94 77-139h-53l18-71Z" fill="#6cffe7"/><rect x="163" y="445" width="175" height="48" rx="20" fill="#1f466b"/><rect x="181" y="457" width="120" height="22" rx="11" fill="#5bffd9"/><path d="M95 535q-80 0-60 82 10 32 55 24" fill="none" stroke="#253951" stroke-width="23" stroke-linecap="round"/><rect x="34" y="620" width="84" height="25" rx="10" fill="#6fe9fc"/><rect x="60" y="639" width="386" height="38" rx="17" fill="#25456b" stroke="#46cde3" stroke-width="7"/></svg>'''
save('charger',charger)
coin='''<svg width="240" height="240" viewBox="0 0 240 240"><defs><radialGradient id="g"><stop stop-color="#fff5a0"/><stop offset=".7" stop-color="#ffcb3d"/><stop offset="1" stop-color="#e89013"/></radialGradient></defs><circle cx="120" cy="120" r="105" fill="#7f450e" opacity=".25"/><circle cx="120" cy="115" r="99" fill="url(#g)" stroke="#ffe6a0" stroke-width="13"/><circle cx="120" cy="115" r="76" fill="none" stroke="#b86a18" stroke-width="8"/><path d="M128 56L83 125h42l-16 55 55-84h-44Z" fill="#fff6cb" stroke="#e48e20" stroke-width="7" stroke-linejoin="round"/></svg>'''
save('energy_coin',coin)
relay='''<svg width="500" height="350" viewBox="0 0 500 350"><defs><linearGradient id="a" x2="0" y2="1"><stop stop-color="#ffe9a4"/><stop offset="1" stop-color="#ff8e40"/></linearGradient></defs><rect x="20" y="97" width="460" height="235" rx="55" fill="#122641" stroke="#60839e" stroke-width="15"/><rect x="46" y="39" width="408" height="214" rx="49" fill="url(#a)" stroke="#fff6d8" stroke-width="14"/><rect x="72" y="85" width="357" height="88" rx="21" fill="#784224"/><path d="M252 62L193 153h55l-22 55 74-107h-61l24-39Z" fill="#fff4bd"/><g fill="#fff5ce"><circle cx="120" cy="290" r="12"/><circle cx="381" cy="290" r="12"/></g></svg>'''
save('relay_switch',relay)
gate='''<svg width="160" height="740" viewBox="0 0 160 740"><defs><linearGradient id="g" x2="1" y2="0"><stop stop-color="#ef5079"/><stop offset=".5" stop-color="#ff98bc"/><stop offset="1" stop-color="#d53f73"/></linearGradient></defs><rect x="33" y="8" width="94" height="721" rx="45" fill="#121e38" stroke="#9d567e" stroke-width="10"/><rect x="50" y="36" width="60" height="650" rx="24" fill="url(#g)"/><g stroke="#ffd5e8" stroke-width="11" opacity=".75"><path d="M47 140l69 45M47 330l69 45M47 520l69 45"/></g></svg>'''
save('energy_gate',gate)
spring='''<svg width="430" height="240" viewBox="0 0 430 240"><defs><linearGradient id="s" x2="0" y2="1"><stop stop-color="#ffec7c"/><stop offset="1" stop-color="#ef7f20"/></linearGradient></defs><path d="M45 206H393" stroke="#28334d" stroke-width="45" stroke-linecap="round"/><path d="M91 170L336 165 106 122 324 92 112 65" fill="none" stroke="#dceefa" stroke-width="26" stroke-linejoin="round"/><rect x="25" y="18" width="380" height="65" rx="24" fill="url(#s)" stroke="#fff6af" stroke-width="11"/></svg>'''
save('spring',spring)
bumper='''<svg width="360" height="360" viewBox="0 0 360 360"><defs><radialGradient id="b"><stop stop-color="#ffe3ab"/><stop offset=".68" stop-color="#fc7857"/><stop offset="1" stop-color="#b72864"/></radialGradient></defs><circle cx="180" cy="180" r="157" fill="url(#b)" stroke="#f9bad1" stroke-width="24"/><circle cx="180" cy="180" r="96" fill="#4f436d" stroke="#f5d4c3" stroke-width="12"/><path d="M140 185h80M180 145v80" stroke="#fff1cf" stroke-width="25" stroke-linecap="round"/></svg>'''
save('bumper',bumper)
print('ART', [p.name for p in root.glob('*.png')])
