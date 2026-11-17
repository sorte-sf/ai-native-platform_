import{a as at,n as Ie,o as jt}from"./chunk-Da7nvo8Y.js";import{C as it,D as ye,S as ie,d as Ot,g as X,h as We,m as U,p as Ti,s as Ft}from"./chunk-I30rBM26.js";import{n as Yt,t as St}from"./chunk-B3RaVecS.js";import{t as a}from"./chunk-B4O1O-ey.js";import{A as le,D as ii,K as sn,N as m,U as ri,X as wt,l as Xs,u as Xt,w as ge}from"./chunk-CS-vcHiB.js";import{_ as Ur,y as Xr}from"./chunk-C877WwUS.js";import{$t as R,Ci as uh,Di as vp,Er as jC,Fr as ko,Ft as OC,H as Fh,Hi as xo,Jn as _v,Kt as Py,Lr as lh,Ni as wo,Nr as jy,Qt as Qy,Rt as O_,Si as ud,Sr as ic,T as Bw,Ti as vE,Vn as Yy,W as GT,Yi as yv,Yr as ne,ei as oe,fi as s_,ft as LB,g as Ao,hi as sv,hr as gd,ht as LT,in as Rs,ir as cr,ji as we,k as D,li as re,lr as e_,mi as sb,mr as fd,o as $M,or as dd,wr as iv,xr as i_,z as FT,zr as lv}from"./chunk-awiZbSKC.js";import{o as w,t as r}from"./chunk-Bhnl8H4w.js";var Lt;var g;var C;var d;var Vt;var Ht;var qt;var Zt;var z;var F;var Qt;var Wt;var Ct;var L=w(()=>{$M();$M();ii();wt();We();ic();St();a();ie();ri();Fh();Lt=Math.pow(2,31)-1,g=class{_overlayRef;instance;containerInstance;_afterDismissed=new ne;_afterOpened=new ne;_onAction=new ne;_durationTimeoutId;_dismissedByAction=!1;constructor(s,t){this._overlayRef=t,this.containerInstance=s,s._onExit.subscribe(()=>this._finishDismiss())}dismiss(){this._afterDismissed.closed||this.containerInstance.exit(),clearTimeout(this._durationTimeoutId)}dismissWithAction(){this._onAction.closed||(this._dismissedByAction=!0,this._onAction.next(),this._onAction.complete(),this.dismiss()),clearTimeout(this._durationTimeoutId)}closeWithAction(){this.dismissWithAction()}_dismissAfter(s){this._durationTimeoutId=setTimeout(()=>this.dismiss(),Math.min(s,Lt))}_open(){this._afterOpened.closed||(this._afterOpened.next(),this._afterOpened.complete())}_finishDismiss(){this._overlayRef.dispose(),this._onAction.closed||this._onAction.complete(),this._afterDismissed.next({dismissedByAction:this._dismissedByAction}),this._afterDismissed.complete(),this._dismissedByAction=!1}afterDismissed(){return this._afterDismissed}afterOpened(){return this.containerInstance._onEnter}onAction(){return this._onAction}},C=new R(`MatSnackBarData`),d=class{politeness=`polite`;announcementMessage=``;viewContainerRef;duration=0;panelClass;direction;data=null;horizontalPosition=`center`;verticalPosition=`bottom`},Vt=(()=>{class i{static ɵfac=function(n){return new(n||i)};static ɵdir=jC({type:i,selectors:[[``,`matSnackBarLabel`,``]],hostAttrs:[1,`mat-mdc-snack-bar-label`,`mdc-snackbar__label`]})}return i})(),Ht=(()=>{class i{static ɵfac=function(n){return new(n||i)};static ɵdir=jC({type:i,selectors:[[``,`matSnackBarActions`,``]],hostAttrs:[1,`mat-mdc-snack-bar-actions`,`mdc-snackbar__actions`]})}return i})(),qt=(()=>{class i{static ɵfac=function(n){return new(n||i)};static ɵdir=jC({type:i,selectors:[[``,`matSnackBarAction`,``]],hostAttrs:[1,`mat-mdc-snack-bar-action`,`mdc-snackbar__action`]})}return i})(),Zt=(()=>{class i{snackBarRef=D(g);data=D(C);action(){this.snackBarRef.dismissWithAction()}get hasAction(){return!!this.data.action}static ɵfac=function(n){return new(n||i)};static ɵcmp=(function(){function t(n,a){if(n&1){let e=GT();Rs(0,`div`,1)(1,`button`,2),iv(`click`,function(){lh(e);let c=e_();return uh(c.action())}),O_(2),ud()()}if(n&2){let e=e_();sb(2),gd(` `,e.data.action,` `)}}return OC({type:i,selectors:[[`simple-snack-bar`]],hostAttrs:[1,`mat-mdc-simple-snack-bar`],exportAs:[`matSnackBar`],decls:3,vars:2,consts:[[`matSnackBarLabel`,``],[`matSnackBarActions`,``],[`matButton`,``,`matSnackBarAction`,``,3,`click`]],template:function(a,e){a&1&&(Rs(0,`div`,0),O_(1),ud(),LT(2,t,3,1,`div`,1)),a&2&&(sb(),gd(` `,e.data.message,`
`),sb(),FT(e.hasAction?2:-1))},dependencies:[Yt,Vt,Ht,qt],styles:[`.mat-mdc-simple-snack-bar {
  display: flex;
}
.mat-mdc-simple-snack-bar .mat-mdc-snack-bar-label {
  max-height: 50vh;
  overflow: auto;
}
`],encapsulation:2})})()}return i})(),z=`_mat-snack-bar-enter`,F=`_mat-snack-bar-exit`,Qt=(()=>{class i extends it{_ngZone=D(we);_elementRef=D(Ao);_changeDetectorRef=D(LB);_platform=D(m);_animationsDisabled=sn();snackBarConfig=D(d);_document=D(cr);_trackedModals=/* @__PURE__ */ new Set;_enterFallback;_exitFallback;_injector=D(re);_announceDelay=150;_announceTimeoutId;_destroyed=!1;_portalOutlet;_onAnnounce=new ne;_onExit=new ne;_onEnter=new ne;_animationState=`void`;_live;_label;_role;_liveElementId=D(ge).getId(`mat-snack-bar-container-live-`);constructor(){super();let t=this.snackBarConfig;t.politeness===`assertive`&&!t.announcementMessage?this._live=`assertive`:t.politeness===`off`?this._live=`off`:this._live=`polite`,this._platform.FIREFOX&&(this._live===`polite`&&(this._role=`status`),this._live===`assertive`&&(this._role=`alert`))}attachComponentPortal(t){this._assertNotAttached();let n=this._portalOutlet.attachComponentPortal(t);return this._afterPortalAttached(),n}attachTemplatePortal(t){this._assertNotAttached();let n=this._portalOutlet.attachTemplatePortal(t);return this._afterPortalAttached(),n}attachDomPortal=t=>{this._assertNotAttached();let n=this._portalOutlet.attachDomPortal(t);return this._afterPortalAttached(),n};onAnimationEnd(t){t===F?this._completeExit():t===z&&(clearTimeout(this._enterFallback),this._ngZone.run(()=>{this._onEnter.next(),this._onEnter.complete()}))}enter(){this._destroyed||(this._animationState=`visible`,this._changeDetectorRef.markForCheck(),this._changeDetectorRef.detectChanges(),this._screenReaderAnnounce(),this._animationsDisabled?Bw(()=>{this._ngZone.run(()=>queueMicrotask(()=>this.onAnimationEnd(z)))},{injector:this._injector}):(clearTimeout(this._enterFallback),this._enterFallback=setTimeout(()=>{this._elementRef.nativeElement.classList.add(`mat-snack-bar-fallback-visible`),this.onAnimationEnd(z)},200)))}exit(){return this._destroyed?vE(void 0):(this._ngZone.run(()=>{this._animationState=`hidden`,this._changeDetectorRef.markForCheck(),this._elementRef.nativeElement.setAttribute(`mat-exit`,``),clearTimeout(this._announceTimeoutId),this._animationsDisabled?Bw(()=>{this._ngZone.run(()=>queueMicrotask(()=>this.onAnimationEnd(F)))},{injector:this._injector}):(clearTimeout(this._exitFallback),this._exitFallback=setTimeout(()=>this.onAnimationEnd(F),200))}),this._onExit)}ngOnDestroy(){this._destroyed=!0,this._clearFromModals(),this._completeExit()}_completeExit(){clearTimeout(this._exitFallback),queueMicrotask(()=>{this._onExit.next(),this._onExit.complete()})}_afterPortalAttached(){let t=this._elementRef.nativeElement,n=this.snackBarConfig.panelClass;n&&(Array.isArray(n)?n.forEach(r=>t.classList.add(r)):t.classList.add(n)),this._exposeToModals();let a=this._label.nativeElement,e=`mdc-snackbar__label`;a.classList.toggle(e,!a.querySelector(`.${e}`))}_exposeToModals(){let t=this._liveElementId,n=this._document.querySelectorAll(`body > .cdk-overlay-container [aria-modal="true"]`);for(let a=0;a<n.length;a++){let e=n[a],r=e.getAttribute(`aria-owns`);this._trackedModals.add(e),r?r.indexOf(t)===-1&&e.setAttribute(`aria-owns`,r+` `+t):e.setAttribute(`aria-owns`,t)}}_clearFromModals(){this._trackedModals.forEach(t=>{let n=t.getAttribute(`aria-owns`);if(n){let a=n.replace(this._liveElementId,``).trim();a.length>0?t.setAttribute(`aria-owns`,a):t.removeAttribute(`aria-owns`)}}),this._trackedModals.clear()}_assertNotAttached(){this._portalOutlet.hasAttached()}_screenReaderAnnounce(){this._announceTimeoutId||this._ngZone.runOutsideAngular(()=>{this._announceTimeoutId=setTimeout(()=>{if(this._destroyed)return;let t=this._elementRef.nativeElement,n=t.querySelector(`[aria-hidden]`),a=t.querySelector(`[aria-live]`);if(n&&a){let e=null;this._platform.isBrowser&&document.activeElement instanceof HTMLElement&&n.contains(document.activeElement)&&(e=document.activeElement),n.removeAttribute(`aria-hidden`),a.appendChild(n),e?.focus(),this._onAnnounce.next(),this._onAnnounce.complete()}},this._announceDelay)})}static ɵfac=function(n){return new(n||i)};static ɵcmp=(function(){let t=[`label`];function n(a,e){}return OC({type:i,selectors:[[`mat-snack-bar-container`]],viewQuery:function(e,r){if(e&1&&lv(Ti,7)(t,7),e&2){let c;i_(c=s_())&&(r._portalOutlet=c.first),i_(c=s_())&&(r._label=c.first)}},hostAttrs:[1,`mdc-snackbar`,`mat-mdc-snack-bar-container`],hostVars:6,hostBindings:function(e,r){e&1&&iv(`animationend`,function(l){return r.onAnimationEnd(l.animationName)})(`animationcancel`,function(l){return r.onAnimationEnd(l.animationName)}),e&2&&yv(`mat-snack-bar-container-enter`,r._animationState===`visible`)(`mat-snack-bar-container-exit`,r._animationState===`hidden`)(`mat-snack-bar-container-animations-enabled`,!r._animationsDisabled)},features:[Py],decls:6,vars:3,consts:[[`label`,``],[1,`mdc-snackbar__surface`,`mat-mdc-snackbar-surface`],[1,`mat-mdc-snack-bar-label`],[`aria-hidden`,`true`],[`cdkPortalOutlet`,``]],template:function(e,r){e&1&&(Rs(0,`div`,1)(1,`div`,2,0)(3,`div`,3),jy(4,n,0,0,`ng-template`,4),ud(),Yy(5,`div`),ud()()),e&2&&(sb(5),Qy(`aria-live`,r._live)(`role`,r._role)(`id`,r._liveElementId))},dependencies:[Ti],styles:[`@keyframes _mat-snack-bar-enter {
  from {
    transform: scale(0.8);
    opacity: 0;
  }
  to {
    transform: scale(1);
    opacity: 1;
  }
}
@keyframes _mat-snack-bar-exit {
  from {
    opacity: 1;
  }
  to {
    opacity: 0;
  }
}
.mat-mdc-snack-bar-container {
  display: flex;
  align-items: center;
  justify-content: center;
  box-sizing: border-box;
  -webkit-tap-highlight-color: rgba(0, 0, 0, 0);
  margin: 8px;
}
.mat-mdc-snack-bar-handset .mat-mdc-snack-bar-container {
  width: 100vw;
}

.mat-snack-bar-container-animations-enabled {
  opacity: 0;
}
.mat-snack-bar-container-animations-enabled.mat-snack-bar-fallback-visible {
  opacity: 1;
}
.mat-snack-bar-container-animations-enabled.mat-snack-bar-container-enter {
  animation: _mat-snack-bar-enter 150ms cubic-bezier(0, 0, 0.2, 1) forwards;
}
.mat-snack-bar-container-animations-enabled.mat-snack-bar-container-exit {
  animation: _mat-snack-bar-exit 75ms cubic-bezier(0.4, 0, 1, 1) forwards;
}

.mat-mdc-snackbar-surface {
  box-shadow: 0px 3px 5px -1px rgba(0, 0, 0, 0.2), 0px 6px 10px 0px rgba(0, 0, 0, 0.14), 0px 1px 18px 0px rgba(0, 0, 0, 0.12);
  display: flex;
  align-items: center;
  justify-content: flex-start;
  box-sizing: border-box;
  padding-left: 0;
  padding-right: 8px;
}
[dir=rtl] .mat-mdc-snackbar-surface {
  padding-right: 0;
  padding-left: 8px;
}
.mat-mdc-snack-bar-container .mat-mdc-snackbar-surface {
  min-width: 344px;
  max-width: 672px;
}
.mat-mdc-snack-bar-handset .mat-mdc-snackbar-surface {
  width: 100%;
  min-width: 0;
}
@media (forced-colors: active) {
  .mat-mdc-snackbar-surface {
    outline: solid 1px;
  }
}
.mat-mdc-snack-bar-container .mat-mdc-snackbar-surface {
  color: var(--%NS%mat-snack-bar-supporting-text-color, var(--%NS%mat-sys-inverse-on-surface));
  border-radius: var(--%NS%mat-snack-bar-container-shape, var(--%NS%mat-sys-corner-extra-small));
  background-color: var(--%NS%mat-snack-bar-container-color, var(--%NS%mat-sys-inverse-surface));
}

.mdc-snackbar__label {
  width: 100%;
  flex-grow: 1;
  box-sizing: border-box;
  margin: 0;
  padding: 14px 8px 14px 16px;
}
[dir=rtl] .mdc-snackbar__label {
  padding-left: 8px;
  padding-right: 16px;
}
.mat-mdc-snack-bar-container .mdc-snackbar__label {
  font-family: var(--%NS%mat-snack-bar-supporting-text-font, var(--%NS%mat-sys-body-medium-font));
  font-size: var(--%NS%mat-snack-bar-supporting-text-size, var(--%NS%mat-sys-body-medium-size));
  font-weight: var(--%NS%mat-snack-bar-supporting-text-weight, var(--%NS%mat-sys-body-medium-weight));
  line-height: var(--%NS%mat-snack-bar-supporting-text-line-height, var(--%NS%mat-sys-body-medium-line-height));
}

.mat-mdc-snack-bar-actions {
  display: flex;
  flex-shrink: 0;
  align-items: center;
  box-sizing: border-box;
}

.mat-mdc-snack-bar-handset,
.mat-mdc-snack-bar-container,
.mat-mdc-snack-bar-label {
  flex: 1 1 auto;
}

.mat-mdc-snack-bar-container .mat-mdc-button.mat-mdc-snack-bar-action:not(:disabled).mat-unthemed {
  color: var(--%NS%mat-snack-bar-button-color, var(--%NS%mat-sys-inverse-primary));
}
.mat-mdc-snack-bar-container .mat-mdc-button.mat-mdc-snack-bar-action:not(:disabled) {
  --%NS%mat-button-text-state-layer-color: currentColor;
  --%NS%mat-button-text-ripple-color: currentColor;
}
.mat-mdc-snack-bar-container .mat-mdc-button.mat-mdc-snack-bar-action:not(:disabled) .mat-ripple-element {
  opacity: 0.1;
}
`],encapsulation:2,changeDetection:1})})()}return i})(),Wt=new R(`mat-snack-bar-default-options`,{providedIn:`root`,factory:()=>new d}),Ct=(()=>{class i{_live=D(Xt);_injector=D(re);_breakpointObserver=D(le);_parentSnackBar=D(i,{optional:!0,skipSelf:!0});_defaultConfig=D(Wt);_animationsDisabled=sn();_snackBarRefAtThisLevel=null;simpleSnackBarComponent=Zt;snackBarContainerComponent=Qt;handsetCssClass=`mat-mdc-snack-bar-handset`;get _openedSnackBarRef(){let t=this._parentSnackBar;return t?t._openedSnackBarRef:this._snackBarRefAtThisLevel}set _openedSnackBarRef(t){this._parentSnackBar?this._parentSnackBar._openedSnackBarRef=t:this._snackBarRefAtThisLevel=t}openFromComponent(t,n){return this._attach(t,n)}openFromTemplate(t,n){return this._attach(t,n)}open(t,n=``,a){let e=r(r({},this._defaultConfig),a);return e.data={message:t,action:n},e.announcementMessage===t&&(e.announcementMessage=void 0),this.openFromComponent(this.simpleSnackBarComponent,e)}dismiss(){this._openedSnackBarRef&&this._openedSnackBarRef.dismiss()}ngOnDestroy(){this._snackBarRefAtThisLevel&&this._snackBarRefAtThisLevel.dismiss()}_attachSnackBarContainer(t,n){let a=n&&n.viewContainerRef&&n.viewContainerRef.injector,e=re.create({parent:a||this._injector,providers:[{provide:d,useValue:n}]}),r=new Ot(this.snackBarContainerComponent,n.viewContainerRef,e),c=t.attach(r);return c.instance.snackBarConfig=n,c.instance}_attach(t,n){let a=r(r(r({},new d),this._defaultConfig),n),e=this._createOverlay(a),r$1=this._attachSnackBarContainer(e,a),c=new g(r$1,e);if(t instanceof wo){let l=new X(t,null,{$implicit:a.data,snackBarRef:c});c.instance=r$1.attachTemplatePortal(l)}else{let l=this._createInjector(a,c),wt=new Ot(t,void 0,l);c.instance=r$1.attachComponentPortal(wt).instance}return this._breakpointObserver.observe(Xs.HandsetPortrait).pipe(vp(e.detachments())).subscribe(l=>{e.overlayElement.classList.toggle(this.handsetCssClass,l.matches)}),a.announcementMessage&&r$1._onAnnounce.subscribe(()=>{this._live.announce(a.announcementMessage,a.politeness)}),this._animateSnackBar(c,a),this._openedSnackBarRef=c,this._openedSnackBarRef}_animateSnackBar(t,n){t.afterDismissed().subscribe(()=>{this._openedSnackBarRef==t&&(this._openedSnackBarRef=null),n.announcementMessage&&this._live.clear()}),n.duration&&n.duration>0&&t.afterOpened().subscribe(()=>t._dismissAfter(n.duration)),this._openedSnackBarRef?(this._openedSnackBarRef.afterDismissed().subscribe(()=>{t.containerInstance.enter()}),this._openedSnackBarRef.dismiss()):t.containerInstance.enter()}_createOverlay(t){let n=new U;n.direction=t.direction;let a=ye(this._injector),e=t.direction===`rtl`,r=t.horizontalPosition===`left`||t.horizontalPosition===`start`&&!e||t.horizontalPosition===`end`&&e,c=!r&&t.horizontalPosition!==`center`;return r?a.left(`0`):c?a.right(`0`):a.centerHorizontally(),t.verticalPosition===`top`?a.top(`0`):a.bottom(`0`),n.positionStrategy=a,n.disableAnimations=this._animationsDisabled,Ft(this._injector,n)}_createInjector(t,n){let a=t&&t.viewContainerRef&&t.viewContainerRef.injector;return re.create({parent:a||this._injector,providers:[{provide:g,useValue:n},{provide:C,useValue:t.data}]})}static ɵfac=function(n){return new(n||i)};static ɵprov=xo({token:i,factory:i.ɵfac})}return i})()});var A;var At=w(()=>{Xr();$M();L();at();$M();A=class i{constructor(s){this.data=s;this.type=this.getType(s.type),this.message=s.message,this.snackbarRef=s.ref}data;type;message;snackbarRef;translocoService=D(Ie);getType(s){switch(s){case`err-snackbar`:return this.translocoService.translate(`notification.error.title`);case`success-snackbar`:return this.translocoService.translate(`snackbar.success`);default:return this.translocoService.translate(`snackbar.info`)}}closeSnackBar(){this.snackbarRef.dismiss()}static ɵfac=function(t){return new(t||i)(ko(C))};static ɵcmp=OC({type:i,selectors:[[`ant-snackbar-dialog`]],decls:7,vars:2,consts:[[1,`snack-dialog-container`,3,`click`],[1,`snackbar-dialog-title`,`text-white`],[1,`snackbar-dialog-content`],[1,`flex`,`flex-row`],[1,`mx-4`]],template:function(t,n){t&1&&(dd(0,`div`,0),sv(`click`,function(){return n.closeSnackBar()}),dd(1,`div`,1),O_(2),fd(),dd(3,`div`,2)(4,`div`,3)(5,`div`,4),O_(6),fd()()()()),t&2&&(sb(2),gd(` `,n.type,` `),sb(4),_v(n.message))},dependencies:[Ur,jt],encapsulation:2})}});var Mt;var Ut=w(()=>{$M();L();at();At();$M();Mt=class i{snackBarConfig;snackBarRef;dismissed;horizontalPosition=`right`;verticalPosition=`top`;snackBarAutoHide=8e3;panelClass=`err-snackbar`;snackBar=D(Ct);translocoService=D(Ie);success(s,t=!0){return this.panelClass=`success-snackbar`,this.openSnackBar(s,t),this.snackBarRef}error(s,t=!0){return this.panelClass=`err-snackbar`,this.openSnackBar(s,t),this.snackBarRef}warning(s,t=!0){return this.panelClass=`default-snackbar`,this.openSnackBar(s,t),this.snackBarRef}info(s,t=!0){return this.panelClass=`info-snackbar`,this.openSnackBar(s,t),this.snackBarRef}pushNotification(s,t=!0){return this.panelClass=`info-snackbar`,this.openSnackBar(s,t),this.snackBarRef}openSnackBar(s,t=!1){return this.snackBarConfig=new d,this.snackBarConfig.horizontalPosition=this.horizontalPosition,this.snackBarConfig.verticalPosition=this.verticalPosition,this.snackBarConfig.duration=this.snackBarAutoHide,this.snackBarConfig.panelClass=this.panelClass,s=t?this.translocoService.translate(s):s,this.snackBarRef=this.snackBar.openFromComponent(A,{horizontalPosition:this.horizontalPosition,verticalPosition:this.verticalPosition,duration:this.snackBarAutoHide,panelClass:this.panelClass,data:{message:s,type:this.panelClass,ref:this.snackBar}}),this.snackBarRef}static ɵfac=function(t){return new(t||i)};static ɵprov=oe({token:i,factory:i.ɵfac,providedIn:`root`})}});export{Ut as n,Mt as t};