import{t as a}from"./chunk-B4O1O-ey.js";import{D as ii,E as ie,G as se,K as sn,N as m,U as ri,b as ci,d as Ye,o as N,t as A}from"./chunk-CS-vcHiB.js";import{n as H,r as L}from"./chunk-CHcIgnsv.js";import{$t as R,Er as jC,Ft as OC,Yi as yv,g as Ao,ji as we,k as D$1,li as re,o as $M}from"./chunk-awiZbSKC.js";import{o as w,t as r}from"./chunk-Bhnl8H4w.js";var at;var et=w(()=>{$M();at=(()=>{class s{static ɵfac=function(n){return new(n||s)};static ɵcmp=OC({type:s,selectors:[[`structural-styles`]],decls:0,vars:0,template:function(n,i){},styles:[`.mat-focus-indicator {
  position: relative;
}
.mat-focus-indicator::before {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
  box-sizing: border-box;
  pointer-events: none;
  display: var(--%NS%mat-focus-indicator-display, none);
  border-width: var(--%NS%mat-focus-indicator-border-width, 3px);
  border-style: var(--%NS%mat-focus-indicator-border-style, solid);
  border-color: var(--%NS%mat-focus-indicator-border-color, transparent);
  border-radius: var(--%NS%mat-focus-indicator-border-radius, 4px);
}
.mat-focus-indicator:focus-visible::before {
  content: "";
}

@media (forced-colors: active) {
  html {
    --%NS%mat-focus-indicator-display: block;
    --%NS%mat-focus-indicator-fallback-border-style: none;
  }
}
`],encapsulation:2})}return s})()});function st(s,t,e){let n=Math.max(Math.abs(s-e.left),Math.abs(s-e.right)),i=Math.max(Math.abs(t-e.top),Math.abs(t-e.bottom));return Math.sqrt(n*n+i*i)}var l;var E;var z;var y;var G;var nt;var B;var V;var Z;var it;var D;var rt;var bt;var ot=w(()=>{a();$M();$M();ii();ci();H();ri();l=(function(s){return s[s.FADING_IN=0]=`FADING_IN`,s[s.VISIBLE=1]=`VISIBLE`,s[s.FADING_OUT=2]=`FADING_OUT`,s[s.HIDDEN=3]=`HIDDEN`,s})(l||{}),E=class{_renderer;element;config;_animationForciblyDisabledThroughCss;state=l.HIDDEN;constructor(t,e,n,i=!1){this._renderer=t,this.element=e,this.config=n,this._animationForciblyDisabledThroughCss=i}fadeOut(){this._renderer.fadeOutRipple(this)}},z=Ye({passive:!0,capture:!0}),y=class{_events=/* @__PURE__ */ new Map;addHandler(t,e,n,i){let r=this._events.get(e);if(r){let d=r.get(n);d?d.add(i):r.set(n,/* @__PURE__ */ new Set([i]))}else this._events.set(e,/* @__PURE__ */ new Map([[n,/* @__PURE__ */ new Set([i])]])),t.runOutsideAngular(()=>{document.addEventListener(e,this._delegateEventHandler,z)})}removeHandler(t,e,n){let i=this._events.get(t);if(!i)return;let r=i.get(e);r&&(r.delete(n),r.size===0&&i.delete(e),i.size===0&&(this._events.delete(t),document.removeEventListener(t,this._delegateEventHandler,z)))}_delegateEventHandler=t=>{let e=A(t);e&&this._events.get(t.type)?.forEach((n,i)=>{(i===e||i.contains(e))&&n.forEach(r=>r.handleEvent(t))})}},G={enterDuration:225,exitDuration:150},nt=800,B=Ye({passive:!0,capture:!0}),V=[`mousedown`,`touchstart`],Z=[`mouseup`,`mouseleave`,`touchend`,`touchcancel`],it=(()=>{class s{static ɵfac=function(n){return new(n||s)};static ɵcmp=OC({type:s,selectors:[[`ng-component`]],hostAttrs:[`mat-ripple-style-loader`,``],decls:0,vars:0,template:function(n,i){},styles:[`.mat-ripple {
  overflow: hidden;
  position: relative;
}
.mat-ripple:not(:empty) {
  transform: translateZ(0);
}

.mat-ripple.mat-ripple-unbounded {
  overflow: visible;
}

.mat-ripple-element {
  position: absolute;
  border-radius: 50%;
  pointer-events: none;
  transition: opacity, transform 0ms cubic-bezier(0, 0, 0.2, 1);
  transform: scale3d(0, 0, 0);
  background-color: var(--%NS%mat-ripple-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 10%, transparent));
}
@media (forced-colors: active) {
  .mat-ripple-element {
    display: none;
  }
}
.cdk-drag-preview .mat-ripple-element, .cdk-drag-placeholder .mat-ripple-element {
  display: none;
}
`],encapsulation:2})}return s})(),D=class s{_target;_ngZone;_platform;_containerElement;_triggerElement=null;_isPointerDown=!1;_activeRipples=/* @__PURE__ */ new Map;_mostRecentTransientRipple=null;_lastTouchStartEvent;_pointerUpEventsRegistered=!1;_containerRect=null;static _eventManager=new y;constructor(t,e,n,i,r){this._target=t,this._ngZone=e,this._platform=i,i.isBrowser&&(this._containerElement=N(n)),r&&r.get(L).load(it)}fadeInRipple(t,e,n={}){let i=this._containerRect=this._containerRect||this._containerElement.getBoundingClientRect(),r$1=r(r({},G),n.animation);n.centered&&(t=i.left+i.width/2,e=i.top+i.height/2);let d=n.radius||st(t,e,i),$=t-i.left,Y=e-i.top,u=r$1.enterDuration,o=document.createElement(`div`);o.classList.add(`mat-ripple-element`),o.style.left=`${$-d}px`,o.style.top=`${Y-d}px`,o.style.height=`${d*2}px`,o.style.width=`${d*2}px`,n.color!=null&&(o.style.backgroundColor=n.color),o.style.transitionDuration=`${u}ms`,this._containerElement.appendChild(o);let T=window.getComputedStyle(o),j=T.transitionProperty,R=T.transitionDuration,f=j===`none`||R===`0s`||R===`0s, 0s`||i.width===0&&i.height===0,p=new E(this,o,n,f);o.style.transform=`scale3d(1, 1, 1)`,p.state=l.FADING_IN,n.persistent||(this._mostRecentTransientRipple=p);let m=null;return!f&&(u||r$1.exitDuration)&&this._ngZone.runOutsideAngular(()=>{let I=()=>{m&&(m.fallbackTimer=null),clearTimeout(w),this._finishRippleTransition(p)},g=()=>this._destroyRipple(p),w=setTimeout(g,u+100);o.addEventListener(`transitionend`,I),o.addEventListener(`transitioncancel`,g),m={onTransitionEnd:I,onTransitionCancel:g,fallbackTimer:w}}),this._activeRipples.set(p,m),(f||!u)&&this._finishRippleTransition(p),p}fadeOutRipple(t){if(t.state===l.FADING_OUT||t.state===l.HIDDEN)return;let e=t.element,n=r(r({},G),t.config.animation);e.style.transitionDuration=`${n.exitDuration}ms`,e.style.opacity=`0`,t.state=l.FADING_OUT,(t._animationForciblyDisabledThroughCss||!n.exitDuration)&&this._finishRippleTransition(t)}fadeOutAll(){this._getActiveRipples().forEach(t=>t.fadeOut())}fadeOutAllNonPersistent(){this._getActiveRipples().forEach(t=>{t.config.persistent||t.fadeOut()})}setupTriggerEvents(t){let e=N(t);!this._platform.isBrowser||!e||e===this._triggerElement||(this._removeTriggerEvents(),this._triggerElement=e,V.forEach(n=>{s._eventManager.addHandler(this._ngZone,n,e,this)}))}handleEvent(t){t.type===`mousedown`?this._onMousedown(t):t.type===`touchstart`?this._onTouchStart(t):this._onPointerUp(),this._pointerUpEventsRegistered||(this._ngZone.runOutsideAngular(()=>{Z.forEach(e=>{this._triggerElement.addEventListener(e,this,B)})}),this._pointerUpEventsRegistered=!0)}_finishRippleTransition(t){t.state===l.FADING_IN?this._startFadeOutTransition(t):t.state===l.FADING_OUT&&this._destroyRipple(t)}_startFadeOutTransition(t){let e=t===this._mostRecentTransientRipple,{persistent:n}=t.config;t.state=l.VISIBLE,!n&&(!e||!this._isPointerDown)&&t.fadeOut()}_destroyRipple(t){let e=this._activeRipples.get(t)??null;this._activeRipples.delete(t),this._activeRipples.size||(this._containerRect=null),t===this._mostRecentTransientRipple&&(this._mostRecentTransientRipple=null),t.state=l.HIDDEN,e!==null&&(t.element.removeEventListener(`transitionend`,e.onTransitionEnd),t.element.removeEventListener(`transitioncancel`,e.onTransitionCancel),e.fallbackTimer!==null&&clearTimeout(e.fallbackTimer)),t.element.remove()}_onMousedown(t){let e=ie(t),n=this._lastTouchStartEvent&&Date.now()<this._lastTouchStartEvent+nt;!this._target.rippleDisabled&&!e&&!n&&(this._isPointerDown=!0,this.fadeInRipple(t.clientX,t.clientY,this._target.rippleConfig))}_onTouchStart(t){if(!this._target.rippleDisabled&&!se(t)){this._lastTouchStartEvent=Date.now(),this._isPointerDown=!0;let e=t.changedTouches;if(e)for(let n=0;n<e.length;n++)this.fadeInRipple(e[n].clientX,e[n].clientY,this._target.rippleConfig)}}_onPointerUp(){this._isPointerDown&&(this._isPointerDown=!1,this._getActiveRipples().forEach(t=>{let e=t.state===l.VISIBLE||t.config.terminateOnPointerUp&&t.state===l.FADING_IN;!t.config.persistent&&e&&t.fadeOut()}))}_getActiveRipples(){return Array.from(this._activeRipples.keys())}_removeTriggerEvents(){let t=this._triggerElement;t&&(V.forEach(e=>s._eventManager.removeHandler(e,t,this)),this._pointerUpEventsRegistered&&(Z.forEach(e=>t.removeEventListener(e,this,B)),this._pointerUpEventsRegistered=!1))}};rt=new R(`mat-ripple-global-options`),bt=(()=>{class s{_elementRef=D$1(Ao);_animationsDisabled=sn();color;unbounded=!1;centered=!1;radius=0;animation;get disabled(){return this._disabled}set disabled(e){e&&this.fadeOutAllNonPersistent(),this._disabled=e,this._setupTriggerEventsIfEnabled()}_disabled=!1;get trigger(){return this._trigger||this._elementRef.nativeElement}set trigger(e){this._trigger=e,this._setupTriggerEventsIfEnabled()}_trigger;_rippleRenderer;_globalOptions;_isInitialized=!1;constructor(){let e=D$1(we),n=D$1(m),i=D$1(rt,{optional:!0}),r=D$1(re);this._globalOptions=i||{},this._rippleRenderer=new D(this,e,this._elementRef,n,r)}ngOnInit(){this._isInitialized=!0,this._setupTriggerEventsIfEnabled()}ngOnDestroy(){this._rippleRenderer._removeTriggerEvents()}fadeOutAll(){this._rippleRenderer.fadeOutAll()}fadeOutAllNonPersistent(){this._rippleRenderer.fadeOutAllNonPersistent()}get rippleConfig(){return{centered:this.centered,radius:this.radius,color:this.color,animation:r(r(r({},this._globalOptions.animation),this._animationsDisabled?{enterDuration:0,exitDuration:0}:{}),this.animation),terminateOnPointerUp:this._globalOptions.terminateOnPointerUp}}get rippleDisabled(){return this.disabled||!!this._globalOptions.disabled}_setupTriggerEventsIfEnabled(){!this.disabled&&this._isInitialized&&this._rippleRenderer.setupTriggerEvents(this.trigger)}launch(e,n=0,i){return typeof e==`number`?this._rippleRenderer.fadeInRipple(e,n,r(r({},this.rippleConfig),i)):this._rippleRenderer.fadeInRipple(0,0,r(r({},this.rippleConfig),e))}static ɵfac=function(n){return new(n||s)};static ɵdir=jC({type:s,selectors:[[``,`mat-ripple`,``],[``,`matRipple`,``]],hostAttrs:[1,`mat-ripple`],hostVars:2,hostBindings:function(n,i){n&2&&yv(`mat-ripple-unbounded`,i.unbounded)},inputs:{color:[0,`matRippleColor`,`color`],unbounded:[0,`matRippleUnbounded`,`unbounded`],centered:[0,`matRippleCentered`,`centered`],radius:[0,`matRippleRadius`,`radius`],animation:[0,`matRippleAnimation`,`animation`],disabled:[0,`matRippleDisabled`,`disabled`],trigger:[0,`matRippleTrigger`,`trigger`]},exportAs:[`matRipple`]})}return s})()});export{et as a,rt as c,bt as i,G as n,l as o,at as r,ot as s,D as t};