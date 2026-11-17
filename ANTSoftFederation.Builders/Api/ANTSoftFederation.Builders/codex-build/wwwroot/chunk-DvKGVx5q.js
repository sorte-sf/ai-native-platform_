import{i as w,n as b,r as h}from"./chunk-BMwwEJ0Q.js";import{a as at,i as Nt}from"./chunk-Da7nvo8Y.js";import{c as Yt,i as Pr,n as Ba,o as Ta}from"./chunk-DX12kyCJ.js";import{t as Vt}from"./chunk-DARvy_nO.js";import{n as gt,r as zt}from"./chunk-DkCplsyW.js";import{S as ie,c as I,f as St,g as X,h as We$1,l as Mt,m as U,n as At,s as Ft,t as Ae}from"./chunk-I30rBM26.js";import{f as yt,i as ae,n as Oe,t as Ee}from"./chunk-CZoCcwUb.js";import{r as Mt$1,u as rt}from"./chunk-_I0KNgok.js";import{i as gt$1,t as St$1}from"./chunk-B3RaVecS.js";import{a as et,i as bt,r as at$1,s as ot}from"./chunk-B_KX_IvN.js";import{t as a}from"./chunk-B4O1O-ey.js";import{C as dn,D as ii,E as ie$1,G as se,J as tt,K as sn,M as lt,N as m,P as mt,T as gt$2,U as ri,X as wt,_ as be,b as ci,k as kt,r as H,v as bt$1,w as ge}from"./chunk-CS-vcHiB.js";import{i as R,n as H$1,r as L,t as A}from"./chunk-CHcIgnsv.js";import{l as Ls,y as Xr}from"./chunk-C877WwUS.js";import{$i as zs,$t as R$1,A as DD,Bn as Yt$1,C as BT,Ci as uh,Cn as Vn$1,Di as vp,Dn as WE,Er as jC,F as Ds,Ft as OC,Gi as ye,H as Fh,Jn as _v,Kn as Zy,Kt as Py,Lr as lh,N as Dh,Ni as wo,Nn as X_,Q as HT,Qn as b_,Qr as nt,Qt as Qy,R as FB,Rr as lt$1,Rt as O_,Si as ud,Sr as ic,T as Bw,Ti as vE,Tn as Vy,V as Ff,Vn as Yy,W as GT,Wn as Z_,Wr as mv,Wt as Pf,X as HI,Yi as yv,Yr as ne$1,Zi as z_,ar as cv,ci as r_,dn as Te,ei as oe,fi as s_,ft as LB,g as Ao,gi as tM,hi as sv,hr as gd,ht as LT,in as Rs,ir as cr,it as Jb,ji as we,k as D,li as re,ln as Sf,lr as e_,mi as sb,mr as fd,o as $M,or as dd,ot as K,qr as n_,rr as c_,sn as SE,ui as rv,vr as he$1,wr as iv,xn as VT,xr as i_,yi as tu,z as FT,zn as Ya,zr as lv}from"./chunk-awiZbSKC.js";import{n as s,o as w$1,t as r}from"./chunk-Bhnl8H4w.js";var Le;var pe;var Dn;var In;var Be;var Me;var te;var Tn;var ee;var En;var Ut;var Zt=w$1(()=>{ie();$M();$M();ic();ii();gt$2();Fh();H$1();et();ot();ri();R();We$1();Le=new R$1(`MAT_MENU_PANEL`),pe=(()=>{class a{_isAnchor;_elementRef=D(Ao);_document=D(cr);_focusMonitor=D(tt);_parentMenu=D(Le,{optional:!0});_changeDetectorRef=D(LB);role=`menuitem`;disabled=!1;disabledInteractive=!1;disableRipple=!1;_hovered=new ne$1;_focused=new ne$1;_highlighted=!1;_triggersSubmenu=!1;constructor(){D(L).load(at$1),this._parentMenu?.addItem?.(this),this._isAnchor=this._elementRef.nativeElement.tagName===`A`}focus(e,t){this._focusMonitor&&e?this._focusMonitor.focusVia(this._getHostElement(),e,t):this._getHostElement().focus(t),this._focused.next(this)}ngAfterViewInit(){this._focusMonitor&&this._focusMonitor.monitor(this._elementRef,!1)}ngOnDestroy(){this._focusMonitor&&this._focusMonitor.stopMonitoring(this._elementRef),this._parentMenu&&this._parentMenu.removeItem&&this._parentMenu.removeItem(this),this._hovered.complete(),this._focused.complete()}_getTabIndex(){return this.disabled&&!this.disabledInteractive?`-1`:`0`}_getAriaDisabled(){return this._isAnchor?this.disabled||null:this.disabled&&this.disabledInteractive?!0:null}_getDisabled(){return this.disabledInteractive||!this.disabled?null:!0}_getHostElement(){return this._elementRef.nativeElement}_checkDisabled(e){this.disabled&&(e.preventDefault(),e.stopPropagation())}_handleMouseEnter(){this._hovered.next(this)}getLabel(){let e=this._elementRef.nativeElement.cloneNode(!0),t=e.querySelectorAll(`mat-icon, .material-icons, .material-symbols-outlined, .material-symbols-rounded, .material-symbols-sharp`);for(let n=0;n<t.length;n++)t[n].remove();return e.textContent?.trim()||``}_setHighlighted(e){this._highlighted=e,this._changeDetectorRef.markForCheck()}_setTriggersSubmenu(e){this._triggersSubmenu=e,this._changeDetectorRef.markForCheck()}_hasFocus(){return this._document&&this._document.activeElement===this._getHostElement()}static ɵfac=function(t){return new(t||a)};static ɵcmp=(function(){let e=[[[`mat-icon`],[``,`matMenuItemIcon`,``]],`*`],t=[`mat-icon, [matMenuItemIcon]`,`*`];function n(r,i){r&1&&(Dh(),Rs(0,`svg`,2),Yy(1,`polygon`,3),ud())}return OC({type:a,selectors:[[``,`mat-menu-item`,``]],hostAttrs:[1,`mat-mdc-menu-item`,`mat-focus-indicator`],hostVars:12,hostBindings:function(i,o){i&1&&iv(`click`,function(p){return o._checkDisabled(p)})(`mouseenter`,function(){return o._handleMouseEnter()}),i&2&&(Qy(`role`,o.role)(`tabindex`,o._getTabIndex())(`aria-disabled`,o._getAriaDisabled())(`disabled`,o._getDisabled()),yv(`mat-mdc-menu-item-highlighted`,o._highlighted)(`mat-mdc-menu-item-submenu-trigger`,o._triggersSubmenu)(`mat-mdc-menu-item-disabled`,o.disabled)(`mat-mdc-menu-item-disabled-interactive`,o.disabledInteractive))},inputs:{role:`role`,disabled:[2,`disabled`,`disabled`,FB],disabledInteractive:[2,`disabledInteractive`,`disabledInteractive`,FB],disableRipple:[2,`disableRipple`,`disableRipple`,FB]},exportAs:[`matMenuItem`],ngContentSelectors:t,decls:5,vars:3,consts:[[1,`mat-mdc-menu-item-text`],[`matRipple`,``,1,`mat-mdc-menu-ripple`,3,`matRippleDisabled`,`matRippleTrigger`],[`viewBox`,`0 0 5 10`,`focusable`,`false`,`aria-hidden`,`true`,1,`mat-mdc-menu-submenu-icon`],[`points`,`0,0 5,5 0,10`]],template:function(i,o){i&1&&(n_(e),r_(0),Rs(1,`span`,0),r_(2,1),ud(),Yy(3,`div`,1),LT(4,n,2,0,`svg`,2)),i&2&&(sb(3),Zy(`matRippleDisabled`,o.disableRipple||o.disabled)(`matRippleTrigger`,o._getHostElement()),sb(),FT(o._triggersSubmenu?4:-1))},dependencies:[bt],encapsulation:2})})()}return a})(),Dn=new R$1(`MatMenuContent`),In=new R$1(`mat-menu-default-options`,{providedIn:`root`,factory:()=>({overlapTrigger:!1,xPosition:`after`,yPosition:`below`,backdropClass:`cdk-overlay-transparent-backdrop`})}),Be=`_mat-menu-enter`,Me=`_mat-menu-exit`,te=(()=>{class a{_elementRef=D(Ao);_changeDetectorRef=D(LB);_injector=D(re);_keyManager;_xPosition;_yPosition;_firstItemFocusRef;_exitFallbackTimeout;_animationsDisabled=sn();_allItems;_directDescendantItems=new Ds;_classList={};_panelAnimationState=`void`;_animationDone=new ne$1;_isAnimating=nt(!1);parentMenu;direction;overlayPanelClass;backdropClass;ariaLabel;ariaLabelledby;ariaDescribedby;get xPosition(){return this._xPosition}set xPosition(e){this._xPosition=e,this.setPositionClasses()}get yPosition(){return this._yPosition}set yPosition(e){this._yPosition=e,this.setPositionClasses()}templateRef;items;lazyContent;overlapTrigger=!1;hasBackdrop;get panelClass(){return this._previousPanelClass}set panelClass(e){let t=this._previousPanelClass,n=r({},this._classList);t&&t.length&&t.split(` `).forEach(r=>{n[r]=!1}),this._previousPanelClass=e,e&&e.length&&(e.split(` `).forEach(r=>{n[r]=!0}),this._elementRef.nativeElement.className=``),this._classList=n}_previousPanelClass=``;get classList(){return this.panelClass}set classList(e){this.panelClass=e}closed=new lt$1;close=this.closed;panelId=D(ge).getId(`mat-menu-panel-`);constructor(){let e=D(In);this.overlayPanelClass=e.overlayPanelClass||``,this._xPosition=e.xPosition,this._yPosition=e.yPosition,this.backdropClass=e.backdropClass,this.overlapTrigger=e.overlapTrigger,this.hasBackdrop=e.hasBackdrop}ngOnInit(){this.setPositionClasses()}ngAfterContentInit(){this._updateDirectDescendants(),this._keyManager=new be(this._directDescendantItems).withWrap().withTypeAhead().withHomeAndEnd().skipPredicate(e=>e.disabled&&!e.disabledInteractive),this._keyManager.tabOut.subscribe(()=>this.closed.emit(`tab`)),this._directDescendantItems.changes.pipe(Pf(this._directDescendantItems),Ff(e=>SE(...e.map(t=>t._focused)))).subscribe(e=>this._keyManager.updateActiveItem(e)),this._directDescendantItems.changes.subscribe(e=>{let t=this._keyManager;if(this._panelAnimationState===`enter`&&t.activeItem?._hasFocus()){let n=e.toArray(),r=Math.max(0,Math.min(n.length-1,t.activeItemIndex||0));n[r]&&!n[r].disabled?t.setActiveItem(r):t.setNextItemActive()}})}ngOnDestroy(){this._keyManager?.destroy(),this._directDescendantItems.destroy(),this.closed.complete(),this._firstItemFocusRef?.destroy(),clearTimeout(this._exitFallbackTimeout)}_hovered(){return this._directDescendantItems.changes.pipe(Pf(this._directDescendantItems),Ff(t=>SE(...t.map(n=>n._hovered))))}addItem(e){}removeItem(e){}_handleKeydown(e){let t=e.keyCode,n=this._keyManager;switch(t){case 27:bt$1(e)||(e.preventDefault(),this.closed.emit(`keydown`));break;case 37:this.parentMenu&&this.direction===`ltr`&&this.closed.emit(`keydown`);break;case 39:this.parentMenu&&this.direction===`rtl`&&this.closed.emit(`keydown`);break;default:(t===38||t===40)&&n.setFocusOrigin(`keyboard`),n.onKeydown(e);return}}focusFirstItem(e=`program`){this._firstItemFocusRef?.destroy(),this._firstItemFocusRef=Bw(()=>{let t=this._resolvePanel();if(!t||!t.contains(document.activeElement)){let n=this._keyManager;n.setFocusOrigin(e).setFirstItemActive(),!n.activeItem&&t&&t.focus()}},{injector:this._injector})}resetActiveItem(){this._keyManager.setActiveItem(-1)}setElevation(e){}setPositionClasses(e=this.xPosition,t=this.yPosition){this._classList=s(r({},this._classList),{"mat-menu-before":e===`before`,"mat-menu-after":e===`after`,"mat-menu-above":t===`above`,"mat-menu-below":t===`below`}),this._changeDetectorRef.markForCheck()}_onAnimationDone(e){let t=e===Me;(t||e===Be)&&(t&&(clearTimeout(this._exitFallbackTimeout),this._exitFallbackTimeout=void 0),this._animationDone.next(t?`void`:`enter`),this._isAnimating.set(!1))}_onAnimationStart(e){(e===Be||e===Me)&&this._isAnimating.set(!0)}_setIsOpen(e){if(this._panelAnimationState=e?`enter`:`void`,e){if(this._keyManager.activeItemIndex===0){let t=this._resolvePanel();t&&(t.scrollTop=0)}}else this._animationsDisabled||(this._exitFallbackTimeout=setTimeout(()=>this._onAnimationDone(Me),200));this._animationsDisabled&&setTimeout(()=>{this._onAnimationDone(e?Be:Me)}),this._changeDetectorRef.markForCheck()}_updateDirectDescendants(){this._allItems.changes.pipe(Pf(this._allItems)).subscribe(e=>{this._directDescendantItems.reset(e.filter(t=>t._parentMenu===this)),this._directDescendantItems.notifyOnChanges()})}_resolvePanel(){let e=null;return this._directDescendantItems.length&&(e=this._directDescendantItems.first._getHostElement().closest(`[role="menu"]`)),e}static ɵfac=function(t){return new(t||a)};static ɵcmp=(function(){let e=[`*`];function t(n,r){if(n&1){let i=GT();dd(0,`div`,0),sv(`click`,function(){lh(i);let d=e_();return uh(d.closed.emit(`click`))})(`animationstart`,function(d){lh(i);let p=e_();return uh(p._onAnimationStart(d.animationName))})(`animationend`,function(d){lh(i);let p=e_();return uh(p._onAnimationDone(d.animationName))})(`animationcancel`,function(d){lh(i);let p=e_();return uh(p._onAnimationDone(d.animationName))}),dd(1,`div`,1),r_(2),fd()()}if(n&2){let i=e_();b_(i._classList),yv(`mat-menu-panel-animations-disabled`,i._animationsDisabled)(`mat-menu-panel-exit-animation`,i._panelAnimationState===`void`)(`mat-menu-panel-animating`,i._isAnimating()),rv(`id`,i.panelId),Qy(`aria-label`,i.ariaLabel||null)(`aria-labelledby`,i.ariaLabelledby||null)(`aria-describedby`,i.ariaDescribedby||null)}}return OC({type:a,selectors:[[`mat-menu`]],contentQueries:function(r,i,o){if(r&1&&cv(o,Dn,5)(o,pe,5)(o,pe,4),r&2){let d;i_(d=s_())&&(i.lazyContent=d.first),i_(d=s_())&&(i._allItems=d),i_(d=s_())&&(i.items=d)}},viewQuery:function(r,i){if(r&1&&lv(wo,5),r&2){let o;i_(o=s_())&&(i.templateRef=o.first)}},hostVars:3,hostBindings:function(r,i){r&2&&Qy(`aria-label`,null)(`aria-labelledby`,null)(`aria-describedby`,null)},inputs:{backdropClass:`backdropClass`,ariaLabel:[0,`aria-label`,`ariaLabel`],ariaLabelledby:[0,`aria-labelledby`,`ariaLabelledby`],ariaDescribedby:[0,`aria-describedby`,`ariaDescribedby`],xPosition:`xPosition`,yPosition:`yPosition`,overlapTrigger:[2,`overlapTrigger`,`overlapTrigger`,FB],hasBackdrop:[2,`hasBackdrop`,`hasBackdrop`,n=>n==null?null:FB(n)],panelClass:[0,`class`,`panelClass`],classList:`classList`},outputs:{closed:`closed`,close:`close`},exportAs:[`matMenu`],features:[z_([{provide:Le,useExisting:a}])],ngContentSelectors:e,decls:1,vars:0,consts:[[`tabindex`,`-1`,`role`,`menu`,1,`mat-mdc-menu-panel`,3,`click`,`animationstart`,`animationend`,`animationcancel`,`id`],[1,`mat-mdc-menu-content`]],template:function(r,i){r&1&&(n_(),Vy(0,t,3,12,`ng-template`))},styles:[`mat-menu {
  display: none;
}

.mat-mdc-menu-content {
  margin: 0;
  padding: 8px 0;
  outline: 0;
}
.mat-mdc-menu-content,
.mat-mdc-menu-content .mat-mdc-menu-item .mat-mdc-menu-item-text {
  -moz-osx-font-smoothing: grayscale;
  -webkit-font-smoothing: antialiased;
  flex: 1;
  white-space: normal;
  font-family: var(--%NS%mat-menu-item-label-text-font, var(--%NS%mat-sys-label-large-font));
  line-height: var(--%NS%mat-menu-item-label-text-line-height, var(--%NS%mat-sys-label-large-line-height));
  font-size: var(--%NS%mat-menu-item-label-text-size, var(--%NS%mat-sys-label-large-size));
  letter-spacing: var(--%NS%mat-menu-item-label-text-tracking, var(--%NS%mat-sys-label-large-tracking));
  font-weight: var(--%NS%mat-menu-item-label-text-weight, var(--%NS%mat-sys-label-large-weight));
}

@keyframes _mat-menu-enter {
  from {
    opacity: 0;
    transform: scale(0.8);
  }
  to {
    opacity: 1;
    transform: none;
  }
}
@keyframes _mat-menu-exit {
  from {
    opacity: 1;
  }
  to {
    opacity: 0;
  }
}
.mat-mdc-menu-panel {
  min-width: 112px;
  max-width: 280px;
  overflow: auto;
  box-sizing: border-box;
  outline: 0;
  animation: _mat-menu-enter 120ms cubic-bezier(0, 0, 0.2, 1);
  border-radius: var(--%NS%mat-menu-container-shape, var(--%NS%mat-sys-corner-extra-small));
  background-color: var(--%NS%mat-menu-container-color, var(--%NS%mat-sys-surface-container));
  box-shadow: var(--%NS%mat-menu-container-elevation-shadow, 0px 3px 1px -2px rgba(0, 0, 0, 0.2), 0px 2px 2px 0px rgba(0, 0, 0, 0.14), 0px 1px 5px 0px rgba(0, 0, 0, 0.12));
  will-change: transform, opacity;
}
.mat-mdc-menu-panel.mat-menu-panel-exit-animation {
  animation: _mat-menu-exit 100ms 25ms linear forwards;
}
.mat-mdc-menu-panel.mat-menu-panel-animations-disabled {
  animation: none;
}
.mat-mdc-menu-panel.mat-menu-panel-animating {
  pointer-events: none;
}
.mat-mdc-menu-panel.mat-menu-panel-animating:has(.mat-mdc-menu-content:empty) {
  display: none;
}
@media (forced-colors: active) {
  .mat-mdc-menu-panel {
    outline: solid 1px;
  }
}
.mat-mdc-menu-panel .mat-divider {
  border-top-color: var(--%NS%mat-menu-divider-color, var(--%NS%mat-sys-surface-variant));
  margin-bottom: var(--%NS%mat-menu-divider-bottom-spacing, 8px);
  margin-top: var(--%NS%mat-menu-divider-top-spacing, 8px);
}

.mat-mdc-menu-item {
  display: flex;
  position: relative;
  align-items: center;
  justify-content: flex-start;
  overflow: hidden;
  padding: 0;
  cursor: pointer;
  width: 100%;
  text-align: left;
  box-sizing: border-box;
  color: inherit;
  font-size: inherit;
  background: none;
  text-decoration: none;
  margin: 0;
  min-height: 48px;
  padding-left: var(--%NS%mat-menu-item-leading-spacing, 12px);
  padding-right: var(--%NS%mat-menu-item-trailing-spacing, 12px);
  -webkit-user-select: none;
  user-select: none;
  cursor: pointer;
  outline: none;
  border: none;
  -webkit-tap-highlight-color: transparent;
}
.mat-mdc-menu-item::-moz-focus-inner {
  border: 0;
}
[dir=rtl] .mat-mdc-menu-item {
  padding-left: var(--%NS%mat-menu-item-trailing-spacing, 12px);
  padding-right: var(--%NS%mat-menu-item-leading-spacing, 12px);
}
.mat-mdc-menu-item:has(.material-icons, .material-symbols-outlined, .material-symbols-rounded, .material-symbols-sharp, mat-icon, [matButtonIcon]) {
  padding-left: var(--%NS%mat-menu-item-with-icon-leading-spacing, 12px);
  padding-right: var(--%NS%mat-menu-item-with-icon-trailing-spacing, 12px);
}
[dir=rtl] .mat-mdc-menu-item:has(.material-icons, .material-symbols-outlined, .material-symbols-rounded, .material-symbols-sharp, mat-icon, [matButtonIcon]) {
  padding-left: var(--%NS%mat-menu-item-with-icon-trailing-spacing, 12px);
  padding-right: var(--%NS%mat-menu-item-with-icon-leading-spacing, 12px);
}
.mat-mdc-menu-item, .mat-mdc-menu-item:visited, .mat-mdc-menu-item:link {
  color: var(--%NS%mat-menu-item-label-text-color, var(--%NS%mat-sys-on-surface));
}
.mat-mdc-menu-item .mat-icon-no-color,
.mat-mdc-menu-item .mat-mdc-menu-submenu-icon {
  color: var(--%NS%mat-menu-item-icon-color, var(--%NS%mat-sys-on-surface-variant));
}
.mat-mdc-menu-item[disabled], .mat-mdc-menu-item.mat-mdc-menu-item-disabled {
  cursor: default;
  opacity: 0.38;
}
.mat-mdc-menu-item[disabled]::after, .mat-mdc-menu-item.mat-mdc-menu-item-disabled::after {
  display: block;
  position: absolute;
  content: "";
  top: 0;
  left: 0;
  bottom: 0;
  right: 0;
}
.mat-mdc-menu-item:focus {
  outline: 0;
}
.mat-mdc-menu-item .mat-icon {
  flex-shrink: 0;
  margin-right: var(--%NS%mat-menu-item-spacing, 12px);
  height: var(--%NS%mat-menu-item-icon-size, 24px);
  width: var(--%NS%mat-menu-item-icon-size, 24px);
}
[dir=rtl] .mat-mdc-menu-item {
  text-align: right;
}
[dir=rtl] .mat-mdc-menu-item .mat-icon {
  margin-right: 0;
  margin-left: var(--%NS%mat-menu-item-spacing, 12px);
}
.mat-mdc-menu-item:not([disabled]):hover {
  background-color: var(--%NS%mat-menu-item-hover-state-layer-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) calc(var(--%NS%mat-sys-hover-state-layer-opacity) * 100%), transparent));
}
.mat-mdc-menu-item:not([disabled]).cdk-program-focused, .mat-mdc-menu-item:not([disabled]).cdk-keyboard-focused, .mat-mdc-menu-item:not([disabled]).mat-mdc-menu-item-highlighted {
  background-color: var(--%NS%mat-menu-item-focus-state-layer-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) calc(var(--%NS%mat-sys-focus-state-layer-opacity) * 100%), transparent));
}
@media (forced-colors: active) {
  .mat-mdc-menu-item {
    margin-top: 1px;
  }
}

.mat-mdc-menu-submenu-icon {
  width: var(--%NS%mat-menu-item-icon-size, 24px);
  height: 10px;
  fill: currentColor;
  padding-left: var(--%NS%mat-menu-item-spacing, 12px);
}
[dir=rtl] .mat-mdc-menu-submenu-icon {
  padding-right: var(--%NS%mat-menu-item-spacing, 12px);
  padding-left: 0;
}
[dir=rtl] .mat-mdc-menu-submenu-icon polygon {
  transform: scaleX(-1);
  transform-origin: center;
}
@media (forced-colors: active) {
  .mat-mdc-menu-submenu-icon {
    fill: CanvasText;
  }
}

.mat-mdc-menu-item .mat-mdc-menu-ripple {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
  pointer-events: none;
}
`],encapsulation:2})})()}return a})(),Tn=new R$1(`mat-menu-scroll-strategy`,{providedIn:`root`,factory:()=>{let a=D(re);return()=>Mt(a)}}),ee=/* @__PURE__ */ new WeakMap,En=(()=>{class a{_canHaveBackdrop;_element=D(Ao);_viewContainerRef=D(zs);_menuItemInstance=D(pe,{optional:!0,self:!0});_dir=D(A,{optional:!0});_focusMonitor=D(tt);_ngZone=D(we);_injector=D(re);_scrollStrategy=D(Tn);_changeDetectorRef=D(LB);_animationsDisabled=sn();_portal;_overlayRef=null;_menuOpen=!1;_closingActionsSubscription=K.EMPTY;_menuCloseSubscription=K.EMPTY;_pendingRemoval;_parentMaterialMenu;_parentInnerPadding;_openedBy=void 0;get _menu(){return this._menuInternal}set _menu(e){e!==this._menuInternal&&(this._menuInternal=e,this._menuCloseSubscription.unsubscribe(),e?(this._parentMaterialMenu,this._menuCloseSubscription=e.close.subscribe(t=>{this._destroyMenu(t),(t===`click`||t===`tab`)&&this._parentMaterialMenu&&this._parentMaterialMenu.closed.emit(t)})):this._destroyMenu(),this._menuItemInstance?._setTriggersSubmenu(this._triggersSubmenu()))}_menuInternal=null;constructor(e){this._canHaveBackdrop=e;let t=D(Le,{optional:!0});this._parentMaterialMenu=t instanceof te?t:void 0}ngOnDestroy(){this._menu&&this._ownsMenu(this._menu)&&ee.delete(this._menu),this._pendingRemoval?.unsubscribe(),this._menuCloseSubscription.unsubscribe(),this._closingActionsSubscription.unsubscribe(),this._overlayRef&&(this._overlayRef.dispose(),this._overlayRef=null)}get menuOpen(){return this._menuOpen}get dir(){return this._dir&&this._dir.value===`rtl`?`rtl`:`ltr`}_triggersSubmenu(){return!!(this._menuItemInstance&&this._parentMaterialMenu&&this._menu)}_closeMenu(){this._menu?.close.emit()}_openMenu(e){if(this._triggerIsAriaDisabled())return;let t=this._menu;if(this._menuOpen||!t)return;this._pendingRemoval?.unsubscribe();let n=ee.get(t);ee.set(t,this),n&&n!==this&&n._closeMenu();let r=this._createOverlay(t),i=r.getConfig(),o=i.positionStrategy;this._setPosition(t,o),this._canHaveBackdrop?i.hasBackdrop=t.hasBackdrop==null?!this._triggersSubmenu():t.hasBackdrop:i.hasBackdrop=t.hasBackdrop??!1,r.hasAttached()||(r.attach(this._getPortal(t)),t.lazyContent?.attach(this.menuData)),this._closingActionsSubscription=this._menuClosingActions().subscribe(()=>this._closeMenu()),t.parentMenu=this._triggersSubmenu()?this._parentMaterialMenu:void 0,t.direction=this.dir,e&&t.focusFirstItem(this._openedBy||`program`),this._setIsMenuOpen(!0),t instanceof te&&(t._setIsOpen(!0),t._directDescendantItems.changes.pipe(vp(t.close)).subscribe(()=>{o.withLockedPosition(!1).reapplyLastPosition(),o.withLockedPosition(!0)}))}focus(e,t){this._focusMonitor&&e?this._focusMonitor.focusVia(this._element,e,t):this._element.nativeElement.focus(t)}_destroyMenu(e){let t=this._overlayRef,n=this._menu;!t||!this.menuOpen||(this._closingActionsSubscription.unsubscribe(),this._pendingRemoval?.unsubscribe(),n instanceof te&&this._ownsMenu(n)?(this._pendingRemoval=n._animationDone.pipe(Yt$1(1)).subscribe(()=>{t.detach(),ee.has(n)||n.lazyContent?.detach()}),n._setIsOpen(!1)):(t.detach(),n?.lazyContent?.detach()),n&&this._ownsMenu(n)&&ee.delete(n),this.restoreFocus&&(e===`keydown`||!this._openedBy||!this._triggersSubmenu())&&this.focus(this._openedBy),this._openedBy=void 0,this._setIsMenuOpen(!1))}_setIsMenuOpen(e){e!==this._menuOpen&&(this._menuOpen=e,this._menuOpen?this.menuOpened.emit():this.menuClosed.emit(),this._triggersSubmenu()&&this._menuItemInstance._setHighlighted(e),this._changeDetectorRef.markForCheck())}_createOverlay(e){if(!this._overlayRef){let t=this._getOverlayConfig(e);this._subscribeToPositions(e,t.positionStrategy),this._overlayRef=Ft(this._injector,t),this._overlayRef.keydownEvents().subscribe(n=>{this._menu instanceof te&&this._menu._handleKeydown(n)})}return this._overlayRef}_getOverlayConfig(e){return new U({positionStrategy:At(this._injector,this._getOverlayOrigin()).withLockedPosition().withGrowAfterOpen().withTransformOriginOn(`.mat-menu-panel, .mat-mdc-menu-panel`),backdropClass:e.backdropClass||`cdk-overlay-transparent-backdrop`,panelClass:e.overlayPanelClass,scrollStrategy:this._scrollStrategy(),direction:this._dir||`ltr`,disableAnimations:this._animationsDisabled})}_subscribeToPositions(e,t){e.setPositionClasses&&t.positionChanges.subscribe(n=>{this._ngZone.run(()=>{let r=n.connectionPair.overlayX===`start`?`after`:`before`,i=n.connectionPair.overlayY===`top`?`below`:`above`;e.setPositionClasses(r,i)})})}_setPosition(e,t){let[n,r]=e.xPosition===`before`?[`end`,`start`]:[`start`,`end`],[i,o]=e.yPosition===`above`?[`bottom`,`top`]:[`top`,`bottom`],[d,p]=[i,o],[h,ke]=[n,r],ie=0;if(this._triggersSubmenu()){if(ke=n=e.xPosition===`before`?`start`:`end`,r=h=n===`end`?`start`:`end`,this._parentMaterialMenu){if(this._parentInnerPadding==null){let Xe=this._parentMaterialMenu.items.first;this._parentInnerPadding=Xe?Xe._getHostElement().offsetTop:0}ie=i===`bottom`?this._parentInnerPadding:-this._parentInnerPadding}}else e.overlapTrigger||(d=i===`top`?`bottom`:`top`,p=o===`top`?`bottom`:`top`);t.withPositions([{originX:n,originY:d,overlayX:h,overlayY:i,offsetY:ie},{originX:r,originY:d,overlayX:ke,overlayY:i,offsetY:ie},{originX:n,originY:p,overlayX:h,overlayY:o,offsetY:-ie},{originX:r,originY:p,overlayX:ke,overlayY:o,offsetY:-ie}])}_menuClosingActions(){let e=this._getOutsideClickStream(this._overlayRef),t=this._overlayRef.detachments(),n=this._parentMaterialMenu?this._parentMaterialMenu.closed:vE(),r=this._parentMaterialMenu?this._parentMaterialMenu._hovered().pipe(Vn$1(i=>this._menuOpen&&i!==this._menuItemInstance)):vE();return SE(e,n,r,t)}_getPortal(e){return(!this._portal||this._portal.templateRef!==e.templateRef)&&(this._portal=new X(e.templateRef,this._viewContainerRef)),this._portal}_ownsMenu(e){return ee.get(e)===this}_triggerIsAriaDisabled(){return FB(this._element.nativeElement.getAttribute(`aria-disabled`))}static ɵfac=function(t){Jb()};static ɵdir=jC({type:a})}return a})(),Ut=(()=>{class a extends En{_cleanupTouchstart;_hoverSubscription=K.EMPTY;get _deprecatedMatMenuTriggerFor(){return this.menu}set _deprecatedMatMenuTriggerFor(e){this.menu=e}get menu(){return this._menu}set menu(e){this._menu=e}menuData;restoreFocus=!0;menuOpened=new lt$1;onMenuOpen=this.menuOpened;menuClosed=new lt$1;onMenuClose=this.menuClosed;constructor(){super(!0);let e=D(tu);this._cleanupTouchstart=e.listen(this._element.nativeElement,`touchstart`,t=>{se(t)||(this._openedBy=`touch`)},{passive:!0})}triggersSubmenu(){return super._triggersSubmenu()}toggleMenu(){return this.menuOpen?this.closeMenu():this.openMenu()}openMenu(){this._openMenu(!0)}closeMenu(){this._closeMenu()}updatePosition(){this._overlayRef?.updatePosition()}ngAfterContentInit(){this._handleHover()}ngOnDestroy(){super.ngOnDestroy(),this._cleanupTouchstart(),this._hoverSubscription.unsubscribe()}_getOverlayOrigin(){return this._element}_getOutsideClickStream(e){return e.backdropClick()}_handleMousedown(e){ie$1(e)||(this._openedBy=e.button===0?`mouse`:void 0,this.triggersSubmenu()&&e.preventDefault())}_handleKeydown(e){let t=e.keyCode;(t===13||t===32)&&(this._openedBy=`keyboard`),this.triggersSubmenu()&&(t===39&&this.dir===`ltr`||t===37&&this.dir===`rtl`)&&(this._openedBy=`keyboard`,this.openMenu())}_handleClick(e){this.triggersSubmenu()?(e.stopPropagation(),this.openMenu()):this.toggleMenu()}_handleHover(){this.triggersSubmenu()&&this._parentMaterialMenu&&(this._hoverSubscription=this._parentMaterialMenu._hovered().subscribe(e=>{e===this._menuItemInstance&&!e.disabled&&this._parentMaterialMenu?._panelAnimationState!==`void`&&(this._openedBy=`mouse`,this._openMenu(!1))}))}static ɵfac=function(t){return new(t||a)};static ɵdir=jC({type:a,selectors:[[``,`mat-menu-trigger-for`,``],[``,`matMenuTriggerFor`,``]],hostAttrs:[1,`mat-mdc-menu-trigger`],hostVars:3,hostBindings:function(t,n){t&1&&iv(`click`,function(i){return n._handleClick(i)})(`mousedown`,function(i){return n._handleMousedown(i)})(`keydown`,function(i){return n._handleKeydown(i)}),t&2&&Qy(`aria-haspopup`,n.menu?`menu`:null)(`aria-expanded`,n.menuOpen)(`aria-controls`,n.menuOpen?n.menu?.panelId:null)},inputs:{_deprecatedMatMenuTriggerFor:[0,`mat-menu-trigger-for`,`_deprecatedMatMenuTriggerFor`],menu:[0,`matMenuTriggerFor`,`menu`],menuData:[0,`matMenuTriggerData`,`menuData`],restoreFocus:[0,`matMenuTriggerRestoreFocus`,`restoreFocus`]},outputs:{menuOpened:`menuOpened`,onMenuOpen:`onMenuOpen`,menuClosed:`menuClosed`,onMenuClose:`onMenuClose`},exportAs:[`matMenuTrigger`],features:[Py]})}return a})()});var Nn;var We;var he;var je;var He;var Ce;var Qe;var $t;var qt=w$1(()=>{ii();R();ci();gt$2();a();St();$M();$M();ic();Fh();ri();Nn=new R$1(`MAT_DRAWER_DEFAULT_AUTOSIZE`,{providedIn:`root`,factory:()=>!1}),We=new R$1(`MAT_DRAWER_CONTAINER`),he=(()=>{class a extends Ae{_platform=D(m);_changeDetectorRef=D(LB);_element=D(Ao);_ngZone=D(we);_isInert=!1;_container=D(He);ngAfterContentInit(){this._container._contentMarginChanges.subscribe(()=>this._changeDetectorRef.markForCheck())}_drawerToggled(e){e.opened?this._ngZone.runOutsideAngular(()=>{e._animationEnd.pipe(WE(50),Yt$1(1)).subscribe(()=>this._updateInert())}):this._updateInert()}_drawerModeChanged(){this._updateInert()}_updateInert(){let e=this._container._isShowingBackdrop();if(e!==this._isInert){let t=this._element.nativeElement;this._isInert=e,e?t.setAttribute(`inert`,`true`):t.removeAttribute(`inert`)}}_shouldBeHidden(){if(this._platform.isBrowser)return!1;let{start:e,end:t}=this._container;return e!=null&&e.mode!==`over`&&e.opened||t!=null&&t.mode!==`over`&&t.opened}static ɵfac=(()=>{let e;return function(n){return(e||(e=DD(a)))(n||a)}})();static ɵcmp=(function(){return OC({type:a,selectors:[[`mat-drawer-content`]],hostAttrs:[1,`mat-drawer-content`],hostVars:6,hostBindings:function(n,r){n&2&&(mv(`margin-left`,r._container._contentMargins.left,`px`)(`margin-right`,r._container._contentMargins.right,`px`),yv(`mat-drawer-content-hidden`,r._shouldBeHidden()))},features:[z_([{provide:Ae,useExisting:a}]),Py],ngContentSelectors:[`*`],decls:1,vars:0,template:function(n,r){n&1&&(n_(),r_(0))},encapsulation:2})})()}return a})(),je=(()=>{class a{_elementRef=D(Ao);_focusTrapFactory=D(mt);_focusMonitor=D(tt);_platform=D(m);_ngZone=D(we);_renderer=D(tu);_interactivityChecker=D(lt);_doc=D(cr);_isAnimating=!1;_container=D(We,{optional:!0});_focusTrap=null;_elementFocusedBeforeDrawerWasOpened=null;_eventCleanups;_isAttached=!1;_anchor=null;get position(){return this._position}set position(e){e=e===`end`?`end`:`start`,e!==this._position&&(this._isAttached&&this._updatePositionInParent(e),this._position=e,this.onPositionChanged.emit())}_position=`start`;get mode(){return this._mode}set mode(e){this._mode=e,this._updateFocusTrapState(),this._modeChanged.next(),this._getContent()?._drawerModeChanged()}_mode=`over`;get disableClose(){return this._disableClose}set disableClose(e){this._disableClose=dn(e)}_disableClose=!1;get autoFocus(){return this._autoFocus??(this.mode===`side`?`dialog`:`first-tabbable`)}set autoFocus(e){(e===`true`||e===`false`||e==null)&&(e=dn(e)),this._autoFocus=e}_autoFocus;get opened(){return this._opened()}set opened(e){this.toggle(dn(e))}_opened=nt(!1);_openedVia=null;_animationStarted=new ne$1;_animationEnd=new ne$1;openedChange=new lt$1(!0);_openedStream=this.openedChange.pipe(Vn$1(e=>e),Te(()=>{}));openedStart=this._animationStarted.pipe(Vn$1(()=>this.opened),Ya(void 0));_closedStream=this.openedChange.pipe(Vn$1(e=>!e),Te(()=>{}));closedStart=this._animationStarted.pipe(Vn$1(()=>!this.opened),Ya(void 0));_destroyed=new ne$1;onPositionChanged=new lt$1;_content;_modeChanged=new ne$1;_injector=D(re);_changeDetectorRef=D(LB);constructor(){this.openedChange.pipe(vp(this._destroyed)).subscribe(e=>{e?(this._elementFocusedBeforeDrawerWasOpened=this._doc.activeElement,this._takeFocus()):this._isFocusWithinDrawer()&&this._restoreFocus(this._openedVia||`program`)}),this._eventCleanups=this._ngZone.runOutsideAngular(()=>{let e=this._renderer,t=this._elementRef.nativeElement;return[e.listen(t,`keydown`,n=>{n.keyCode===27&&!this.disableClose&&!bt$1(n)&&this._ngZone.run(()=>{this.close(),n.stopPropagation(),n.preventDefault()})}),e.listen(t,`transitionend`,this._handleTransitionEvent),e.listen(t,`transitioncancel`,this._handleTransitionEvent)]}),this._animationEnd.subscribe(()=>{this.openedChange.emit(this.opened)})}_focusByCssSelector(e,t){let n=this._elementRef.nativeElement.querySelector(e);n&&(this._interactivityChecker.isFocusable(n)||(n.tabIndex=-1,this._ngZone.runOutsideAngular(()=>{let r=()=>{i(),o(),n.removeAttribute(`tabindex`)},i=this._renderer.listen(n,`blur`,r),o=this._renderer.listen(n,`mousedown`,r)})),n.focus(t))}_takeFocus(){if(!this._focusTrap)return;let e=this._elementRef.nativeElement;switch(this.autoFocus){case!1:case`dialog`:return;case!0:case`first-tabbable`:Bw(()=>{let t=this._isAnimating?{preventScroll:!0}:void 0;!this._focusTrap.focusInitialElement(t)&&typeof e.focus==`function`&&e.focus(t)},{injector:this._injector});break;case`first-heading`:this._focusByCssSelector(`h1, h2, h3, h4, h5, h6, [role="heading"]`);break;default:this._focusByCssSelector(this.autoFocus)}}_restoreFocus(e){this.autoFocus!==`dialog`&&(this._elementFocusedBeforeDrawerWasOpened?this._focusMonitor.focusVia(this._elementFocusedBeforeDrawerWasOpened,e):this._elementRef.nativeElement.blur(),this._elementFocusedBeforeDrawerWasOpened=null)}_isFocusWithinDrawer(){let e=this._doc.activeElement;return!!e&&this._elementRef.nativeElement.contains(e)}ngAfterViewInit(){this._isAttached=!0,this._position===`end`&&this._updatePositionInParent(`end`),this._platform.isBrowser&&(this._focusTrap=this._focusTrapFactory.create(this._elementRef.nativeElement),this._updateFocusTrapState())}ngOnDestroy(){this._eventCleanups.forEach(e=>e()),this._focusTrap?.destroy(),this._anchor?.remove(),this._anchor=null,this._animationStarted.complete(),this._animationEnd.complete(),this._modeChanged.complete(),this._destroyed.next(),this._destroyed.complete()}open(e){return this.toggle(!0,e)}close(){return this.toggle(!1)}_closeViaBackdropClick(){return this._setOpen(!1,!0,`mouse`)}toggle(e=!this.opened,t){e&&t&&(this._openedVia=t);let n=this._setOpen(e,!e&&this._isFocusWithinDrawer(),this._openedVia||`program`);return e||(this._openedVia=null),n}_setOpen(e,t,n){return e===this.opened?Promise.resolve(e?`open`:`close`):(this._opened.set(e),this._getContent()?._drawerToggled(this),this._container?._transitionsEnabled?this._isAnimating?(this._setIsAnimating(!1),this._simulateAnimation()):(this._setIsAnimating(!0),setTimeout(()=>this._animationStarted.next())):this._simulateAnimation(),this._elementRef.nativeElement.classList.toggle(`mat-drawer-opened`,e),!e&&t&&this._restoreFocus(n),this._changeDetectorRef.markForCheck(),this._updateFocusTrapState(),new Promise(r=>{this.openedChange.pipe(Yt$1(1)).subscribe(i=>r(i?`open`:`close`))}))}_getContent(){return this._container?._content||this._container?._userContent}_setIsAnimating(e){e!==this._isAnimating&&(this._isAnimating=e,this._elementRef.nativeElement.classList.toggle(`mat-drawer-animating`,e))}_simulateAnimation(){setTimeout(()=>{this._animationStarted.next(),this._animationEnd.next()})}_getWidth(){return this._elementRef.nativeElement.offsetWidth||0}_updateFocusTrapState(){this._focusTrap&&(this._focusTrap.enabled=this.opened&&!!this._container?._isShowingBackdrop())}_updatePositionInParent(e){if(!this._platform.isBrowser)return;let t=this._elementRef.nativeElement,n=t.parentNode;e===`end`?(this._anchor||(this._anchor=this._doc.createComment(`mat-drawer-anchor`),n.insertBefore(this._anchor,t)),n.appendChild(t)):this._anchor&&this._anchor.parentNode.insertBefore(t,this._anchor)}_handleTransitionEvent=e=>{let t=this._elementRef.nativeElement;e.target===t&&this._ngZone.run(()=>{e.type===`transitionend`&&this._setIsAnimating(!1),this._animationEnd.next(e)})};static ɵfac=function(t){return new(t||a)};static ɵcmp=(function(){let e=[`content`];return OC({type:a,selectors:[[`mat-drawer`]],viewQuery:function(r,i){if(r&1&&lv(e,5),r&2){let o;i_(o=s_())&&(i._content=o.first)}},hostAttrs:[1,`mat-drawer`],hostVars:12,hostBindings:function(r,i){r&2&&(Qy(`align`,null)(`tabIndex`,i.mode!==`side`?`-1`:null),mv(`visibility`,!i._container&&!i.opened?`hidden`:null),yv(`mat-drawer-end`,i.position===`end`)(`mat-drawer-over`,i.mode===`over`)(`mat-drawer-push`,i.mode===`push`)(`mat-drawer-side`,i.mode===`side`))},inputs:{position:`position`,mode:`mode`,disableClose:`disableClose`,autoFocus:`autoFocus`,opened:`opened`},outputs:{openedChange:`openedChange`,_openedStream:`opened`,openedStart:`openedStart`,_closedStream:`closed`,closedStart:`closedStart`,onPositionChanged:`positionChanged`},exportAs:[`matDrawer`],ngContentSelectors:[`*`],decls:3,vars:0,consts:[[`content`,``],[`cdkScrollable`,``,1,`mat-drawer-inner-container`]],template:function(r,i){r&1&&(n_(),Rs(0,`div`,1,0),r_(2),ud())},dependencies:[Ae],encapsulation:2})})()}return a})(),He=(()=>{class a{_dir=D(A,{optional:!0});_element=D(Ao);_ngZone=D(we);_changeDetectorRef=D(LB);_animationDisabled=sn();_transitionsEnabled=!1;_allDrawers;_drawers=new Ds;_content;_userContent;get start(){return this._start}get end(){return this._end}get autosize(){return this._autosize}set autosize(e){this._autosize=dn(e)}_autosize=D(Nn);get hasBackdrop(){return this._drawerHasBackdrop(this._start)||this._drawerHasBackdrop(this._end)}set hasBackdrop(e){this._backdropOverride=e==null?null:dn(e)}_backdropOverride=null;backdropClick=new lt$1;_start=null;_end=null;_left=null;_right=null;_destroyed=new ne$1;_doCheckSubject=new ne$1;_contentMargins={left:null,right:null};_contentMarginChanges=new ne$1;get scrollable(){return this._userContent||this._content}_injector=D(re);constructor(){let e=D(m),t=D(I);this._dir?.change.pipe(vp(this._destroyed)).subscribe(()=>{this._validateDrawers(),this.updateContentMargins()}),t.change().pipe(vp(this._destroyed)).subscribe(()=>this.updateContentMargins()),!this._animationDisabled&&e.isBrowser&&this._ngZone.runOutsideAngular(()=>{setTimeout(()=>{this._element.nativeElement.classList.add(`mat-drawer-transition`),this._transitionsEnabled=!0},200)})}ngAfterContentInit(){this._allDrawers.changes.pipe(Pf(this._allDrawers),vp(this._destroyed)).subscribe(e=>{this._drawers.reset(e.filter(t=>!t._container||t._container===this)),this._drawers.notifyOnChanges()}),this._drawers.changes.pipe(Pf(null)).subscribe(()=>{this._validateDrawers(),this._drawers.forEach(e=>{this._watchDrawerToggle(e),this._watchDrawerPosition(e),this._watchDrawerMode(e)}),(!this._drawers.length||this._isDrawerOpen(this._start)||this._isDrawerOpen(this._end))&&this.updateContentMargins(),this._changeDetectorRef.markForCheck()}),this._ngZone.runOutsideAngular(()=>{this._doCheckSubject.pipe(Sf(10),vp(this._destroyed)).subscribe(()=>this.updateContentMargins())})}ngOnDestroy(){this._contentMarginChanges.complete(),this._doCheckSubject.complete(),this._drawers.destroy(),this._destroyed.next(),this._destroyed.complete()}open(){this._drawers.forEach(e=>e.open())}close(){this._drawers.forEach(e=>e.close())}updateContentMargins(){let e=0,t=0;if(this._left&&this._left.opened){if(this._left.mode==`side`)e+=this._left._getWidth();else if(this._left.mode==`push`){let n=this._left._getWidth();e+=n,t-=n}}if(this._right&&this._right.opened){if(this._right.mode==`side`)t+=this._right._getWidth();else if(this._right.mode==`push`){let n=this._right._getWidth();t+=n,e-=n}}e=e||null,t=t||null,(e!==this._contentMargins.left||t!==this._contentMargins.right)&&(this._contentMargins={left:e,right:t},this._ngZone.run(()=>this._contentMarginChanges.next(this._contentMargins)))}ngDoCheck(){this._autosize&&this._isPushed()&&this._ngZone.runOutsideAngular(()=>this._doCheckSubject.next())}_watchDrawerToggle(e){e._animationStarted.pipe(vp(this._drawers.changes)).subscribe(()=>{this.updateContentMargins(),this._changeDetectorRef.markForCheck()}),e.mode!==`side`&&e.openedChange.pipe(vp(this._drawers.changes)).subscribe(()=>this._setContainerClass(e.opened))}_watchDrawerPosition(e){e.onPositionChanged.pipe(vp(this._drawers.changes)).subscribe(()=>{Bw({read:()=>this._validateDrawers()},{injector:this._injector})})}_watchDrawerMode(e){e._modeChanged.pipe(vp(SE(this._drawers.changes,this._destroyed))).subscribe(()=>{this.updateContentMargins(),this._changeDetectorRef.markForCheck()})}_setContainerClass(e){let t=this._element.nativeElement.classList,n=`mat-drawer-container-has-open`;e?t.add(n):t.remove(n)}_validateDrawers(){this._start=this._end=null,this._drawers.forEach(e=>{e.position==`end`?(this._end,this._end=e):(this._start,this._start=e)}),this._right=this._left=null,this._dir&&this._dir.value===`rtl`?(this._left=this._end,this._right=this._start):(this._left=this._start,this._right=this._end)}_isPushed(){return this._isDrawerOpen(this._start)&&this._start.mode!=`over`||this._isDrawerOpen(this._end)&&this._end.mode!=`over`}_onBackdropClicked(){this.backdropClick.emit(),this._closeModalDrawersViaBackdrop()}_closeModalDrawersViaBackdrop(){[this._start,this._end].filter(e=>e&&!e.disableClose&&this._drawerHasBackdrop(e)).forEach(e=>e._closeViaBackdropClick())}_isShowingBackdrop(){return this._isDrawerOpen(this._start)&&this._drawerHasBackdrop(this._start)||this._isDrawerOpen(this._end)&&this._drawerHasBackdrop(this._end)}_isDrawerOpen(e){return e!=null&&e.opened}_drawerHasBackdrop(e){return this._backdropOverride==null?!!e&&e.mode!==`side`:this._backdropOverride}static ɵfac=function(t){return new(t||a)};static ɵcmp=(function(){let e=[[[`mat-drawer`],[`mat-sidenav`]],[[`mat-drawer-content`],[`mat-sidenav-content`]],`*`],t=[`mat-drawer, mat-sidenav`,`mat-drawer-content, mat-sidenav-content`,`*`];function n(i,o){if(i&1){let d=GT();Rs(0,`div`,1),iv(`click`,function(){lh(d);let h=e_();return uh(h._onBackdropClicked())}),ud()}if(i&2){let d=e_();yv(`mat-drawer-shown`,d._isShowingBackdrop())}}function r(i,o){i&1&&(Rs(0,`mat-drawer-content`),r_(1,2),ud())}return OC({type:a,selectors:[[`mat-drawer-container`]],contentQueries:function(o,d,p){if(o&1&&cv(p,he,5)(p,je,5),o&2){let h;i_(h=s_())&&(d._content=h.first),i_(h=s_())&&(d._allDrawers=h)}},viewQuery:function(o,d){if(o&1&&lv(he,5),o&2){let p;i_(p=s_())&&(d._userContent=p.first)}},hostAttrs:[1,`mat-drawer-container`],hostVars:2,hostBindings:function(o,d){o&2&&yv(`mat-drawer-container-explicit-backdrop`,d._backdropOverride)},inputs:{autosize:`autosize`,hasBackdrop:`hasBackdrop`},outputs:{backdropClick:`backdropClick`},exportAs:[`matDrawerContainer`],features:[z_([{provide:We,useExisting:a}])],ngContentSelectors:t,decls:4,vars:2,consts:[[1,`mat-drawer-backdrop`,3,`mat-drawer-shown`],[1,`mat-drawer-backdrop`,3,`click`]],template:function(o,d){o&1&&(n_(e),LT(0,n,1,2,`div`,0),r_(1),r_(2,1),LT(3,r,2,0,`mat-drawer-content`)),o&2&&(FT(d.hasBackdrop?0:-1),sb(3),FT(d._content?-1:3))},dependencies:[he],styles:[`.mat-drawer-container {
  position: relative;
  z-index: 1;
  color: var(--%NS%mat-sidenav-content-text-color, var(--%NS%mat-sys-on-background));
  background-color: var(--%NS%mat-sidenav-content-background-color, var(--%NS%mat-sys-background));
  box-sizing: border-box;
  display: block;
  overflow: hidden;
}
.mat-drawer-container[fullscreen] {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
}
.mat-drawer-container[fullscreen].mat-drawer-container-has-open {
  overflow: hidden;
}
.mat-drawer-container.mat-drawer-container-explicit-backdrop .mat-drawer-side {
  z-index: 3;
}
.mat-drawer-container.ng-animate-disabled .mat-drawer-backdrop,
.mat-drawer-container.ng-animate-disabled .mat-drawer-content, .ng-animate-disabled .mat-drawer-container .mat-drawer-backdrop,
.ng-animate-disabled .mat-drawer-container .mat-drawer-content {
  transition: none;
}

.mat-drawer-backdrop {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
  display: block;
  z-index: 3;
  visibility: hidden;
}
.mat-drawer-backdrop.mat-drawer-shown {
  visibility: visible;
  background-color: var(--%NS%mat-sidenav-scrim-color, color-mix(in srgb, var(--%NS%mat-sys-neutral-variant20) 40%, transparent));
}
.mat-drawer-transition .mat-drawer-backdrop {
  transition-duration: 400ms;
  transition-timing-function: cubic-bezier(0.25, 0.8, 0.25, 1);
  transition-property: background-color, visibility;
}
@media (forced-colors: active) {
  .mat-drawer-backdrop {
    opacity: 0.5;
  }
}

.mat-drawer-content {
  position: relative;
  z-index: 1;
  display: block;
  height: 100%;
  overflow: auto;
}
.mat-drawer-content.mat-drawer-content-hidden {
  opacity: 0;
}
.mat-drawer-transition .mat-drawer-content {
  transition-duration: 400ms;
  transition-timing-function: cubic-bezier(0.25, 0.8, 0.25, 1);
  transition-property: transform, margin-left, margin-right;
}

.mat-drawer {
  position: relative;
  z-index: 4;
  color: var(--%NS%mat-sidenav-container-text-color, var(--%NS%mat-sys-on-surface-variant));
  box-shadow: var(--%NS%mat-sidenav-container-elevation-shadow, none);
  background-color: var(--%NS%mat-sidenav-container-background-color, var(--%NS%mat-sys-surface));
  border-top-right-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-bottom-right-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  width: var(--%NS%mat-sidenav-container-width, 360px);
  display: block;
  position: absolute;
  top: 0;
  bottom: 0;
  z-index: 3;
  outline: 0;
  box-sizing: border-box;
  overflow-y: auto;
  transform: translate3d(-100%, 0, 0);
}
@media (forced-colors: active) {
  .mat-drawer, [dir=rtl] .mat-drawer.mat-drawer-end {
    border-right: solid 1px currentColor;
  }
}
@media (forced-colors: active) {
  [dir=rtl] .mat-drawer, .mat-drawer.mat-drawer-end {
    border-left: solid 1px currentColor;
    border-right: none;
  }
}
.mat-drawer.mat-drawer-side {
  z-index: 2;
}
.mat-drawer.mat-drawer-end {
  right: 0;
  transform: translate3d(100%, 0, 0);
  border-top-left-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-bottom-left-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-top-right-radius: 0;
  border-bottom-right-radius: 0;
}
[dir=rtl] .mat-drawer {
  border-top-left-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-bottom-left-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-top-right-radius: 0;
  border-bottom-right-radius: 0;
  transform: translate3d(100%, 0, 0);
}
[dir=rtl] .mat-drawer.mat-drawer-end {
  border-top-right-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-bottom-right-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-top-left-radius: 0;
  border-bottom-left-radius: 0;
  left: 0;
  right: auto;
  transform: translate3d(-100%, 0, 0);
}
.mat-drawer-transition .mat-drawer {
  transition: transform 400ms cubic-bezier(0.25, 0.8, 0.25, 1);
}
.mat-drawer:not(.mat-drawer-opened):not(.mat-drawer-animating) {
  visibility: hidden;
  box-shadow: none;
}
.mat-drawer:not(.mat-drawer-opened):not(.mat-drawer-animating) .mat-drawer-inner-container {
  display: none;
}
.mat-drawer.mat-drawer-opened.mat-drawer-opened {
  transform: none;
}

.mat-drawer-side {
  box-shadow: none;
  border-right-color: var(--%NS%mat-sidenav-container-divider-color, transparent);
  border-right-width: 1px;
  border-right-style: solid;
}
.mat-drawer-side.mat-drawer-end {
  border-left-color: var(--%NS%mat-sidenav-container-divider-color, transparent);
  border-left-width: 1px;
  border-left-style: solid;
  border-right: none;
}
[dir=rtl] .mat-drawer-side {
  border-left-color: var(--%NS%mat-sidenav-container-divider-color, transparent);
  border-left-width: 1px;
  border-left-style: solid;
  border-right: none;
}
[dir=rtl] .mat-drawer-side.mat-drawer-end {
  border-right-color: var(--%NS%mat-sidenav-container-divider-color, transparent);
  border-right-width: 1px;
  border-right-style: solid;
  border-left: none;
}

.mat-drawer-inner-container {
  width: 100%;
  height: 100%;
  overflow: auto;
}

.mat-sidenav-fixed {
  position: fixed;
}
`],encapsulation:2})})()}return a})(),Ce=(()=>{class a extends he{static ɵfac=(()=>{let e;return function(n){return(e||(e=DD(a)))(n||a)}})();static ɵcmp=(function(){return OC({type:a,selectors:[[`mat-sidenav-content`]],hostAttrs:[1,`mat-drawer-content`,`mat-sidenav-content`],features:[z_([{provide:Ae,useExisting:a},{provide:he,useExisting:a}]),Py],ngContentSelectors:[`*`],decls:1,vars:0,template:function(n,r){n&1&&(n_(),r_(0))},encapsulation:2})})()}return a})(),Qe=(()=>{class a extends je{get fixedInViewport(){return this._fixedInViewport}set fixedInViewport(e){this._fixedInViewport=dn(e)}_fixedInViewport=!1;get fixedTopGap(){return this._fixedTopGap}set fixedTopGap(e){this._fixedTopGap=kt(e)}_fixedTopGap=0;get fixedBottomGap(){return this._fixedBottomGap}set fixedBottomGap(e){this._fixedBottomGap=kt(e)}_fixedBottomGap=0;static ɵfac=(()=>{let e;return function(n){return(e||(e=DD(a)))(n||a)}})();static ɵcmp=(function(){return OC({type:a,selectors:[[`mat-sidenav`]],hostAttrs:[1,`mat-drawer`,`mat-sidenav`],hostVars:16,hostBindings:function(n,r){n&2&&(Qy(`tabIndex`,r.mode!==`side`?`-1`:null)(`align`,null),mv(`top`,r.fixedInViewport?r.fixedTopGap:null,`px`)(`bottom`,r.fixedInViewport?r.fixedBottomGap:null,`px`),yv(`mat-drawer-end`,r.position===`end`)(`mat-drawer-over`,r.mode===`over`)(`mat-drawer-push`,r.mode===`push`)(`mat-drawer-side`,r.mode===`side`)(`mat-sidenav-fixed`,r.fixedInViewport))},inputs:{fixedInViewport:`fixedInViewport`,fixedTopGap:`fixedTopGap`,fixedBottomGap:`fixedBottomGap`},exportAs:[`matSidenav`],features:[z_([{provide:je,useExisting:a}]),Py],ngContentSelectors:[`*`],decls:3,vars:0,consts:[[`content`,``],[`cdkScrollable`,``,1,`mat-drawer-inner-container`]],template:function(n,r){n&1&&(n_(),Rs(0,`div`,1,0),r_(2),ud())},dependencies:[Ae],encapsulation:2})})()}return a})(),$t=(()=>{class a extends He{_allDrawers=void 0;_content=void 0;static ɵfac=(()=>{let e;return function(n){return(e||(e=DD(a)))(n||a)}})();static ɵcmp=(function(){let e=[[[`mat-drawer`],[`mat-sidenav`]],[[`mat-drawer-content`],[`mat-sidenav-content`]],`*`],t=[`mat-drawer, mat-sidenav`,`mat-drawer-content, mat-sidenav-content`,`*`];function n(i,o){if(i&1){let d=GT();Rs(0,`div`,1),iv(`click`,function(){lh(d);let h=e_();return uh(h._onBackdropClicked())}),ud()}if(i&2){let d=e_();yv(`mat-drawer-shown`,d._isShowingBackdrop())}}function r(i,o){i&1&&(Rs(0,`mat-sidenav-content`),r_(1,2),ud())}return OC({type:a,selectors:[[`mat-sidenav-container`]],contentQueries:function(o,d,p){if(o&1&&cv(p,Ce,5)(p,Qe,5),o&2){let h;i_(h=s_())&&(d._content=h.first),i_(h=s_())&&(d._allDrawers=h)}},hostAttrs:[1,`mat-drawer-container`,`mat-sidenav-container`],hostVars:2,hostBindings:function(o,d){o&2&&yv(`mat-drawer-container-explicit-backdrop`,d._backdropOverride)},exportAs:[`matSidenavContainer`],features:[z_([{provide:We,useExisting:a},{provide:He,useExisting:a}]),Py],ngContentSelectors:t,decls:4,vars:2,consts:[[1,`mat-drawer-backdrop`,3,`mat-drawer-shown`],[1,`mat-drawer-backdrop`,3,`click`]],template:function(o,d){o&1&&(n_(e),LT(0,n,1,2,`div`,0),r_(1),r_(2,1),LT(3,r,2,0,`mat-sidenav-content`)),o&2&&(FT(d.hasBackdrop?0:-1),sb(3),FT(d._content?-1:3))},dependencies:[Ce],styles:[`.mat-drawer-container {
  position: relative;
  z-index: 1;
  color: var(--%NS%mat-sidenav-content-text-color, var(--%NS%mat-sys-on-background));
  background-color: var(--%NS%mat-sidenav-content-background-color, var(--%NS%mat-sys-background));
  box-sizing: border-box;
  display: block;
  overflow: hidden;
}
.mat-drawer-container[fullscreen] {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
}
.mat-drawer-container[fullscreen].mat-drawer-container-has-open {
  overflow: hidden;
}
.mat-drawer-container.mat-drawer-container-explicit-backdrop .mat-drawer-side {
  z-index: 3;
}
.mat-drawer-container.ng-animate-disabled .mat-drawer-backdrop,
.mat-drawer-container.ng-animate-disabled .mat-drawer-content, .ng-animate-disabled .mat-drawer-container .mat-drawer-backdrop,
.ng-animate-disabled .mat-drawer-container .mat-drawer-content {
  transition: none;
}

.mat-drawer-backdrop {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
  display: block;
  z-index: 3;
  visibility: hidden;
}
.mat-drawer-backdrop.mat-drawer-shown {
  visibility: visible;
  background-color: var(--%NS%mat-sidenav-scrim-color, color-mix(in srgb, var(--%NS%mat-sys-neutral-variant20) 40%, transparent));
}
.mat-drawer-transition .mat-drawer-backdrop {
  transition-duration: 400ms;
  transition-timing-function: cubic-bezier(0.25, 0.8, 0.25, 1);
  transition-property: background-color, visibility;
}
@media (forced-colors: active) {
  .mat-drawer-backdrop {
    opacity: 0.5;
  }
}

.mat-drawer-content {
  position: relative;
  z-index: 1;
  display: block;
  height: 100%;
  overflow: auto;
}
.mat-drawer-content.mat-drawer-content-hidden {
  opacity: 0;
}
.mat-drawer-transition .mat-drawer-content {
  transition-duration: 400ms;
  transition-timing-function: cubic-bezier(0.25, 0.8, 0.25, 1);
  transition-property: transform, margin-left, margin-right;
}

.mat-drawer {
  position: relative;
  z-index: 4;
  color: var(--%NS%mat-sidenav-container-text-color, var(--%NS%mat-sys-on-surface-variant));
  box-shadow: var(--%NS%mat-sidenav-container-elevation-shadow, none);
  background-color: var(--%NS%mat-sidenav-container-background-color, var(--%NS%mat-sys-surface));
  border-top-right-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-bottom-right-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  width: var(--%NS%mat-sidenav-container-width, 360px);
  display: block;
  position: absolute;
  top: 0;
  bottom: 0;
  z-index: 3;
  outline: 0;
  box-sizing: border-box;
  overflow-y: auto;
  transform: translate3d(-100%, 0, 0);
}
@media (forced-colors: active) {
  .mat-drawer, [dir=rtl] .mat-drawer.mat-drawer-end {
    border-right: solid 1px currentColor;
  }
}
@media (forced-colors: active) {
  [dir=rtl] .mat-drawer, .mat-drawer.mat-drawer-end {
    border-left: solid 1px currentColor;
    border-right: none;
  }
}
.mat-drawer.mat-drawer-side {
  z-index: 2;
}
.mat-drawer.mat-drawer-end {
  right: 0;
  transform: translate3d(100%, 0, 0);
  border-top-left-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-bottom-left-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-top-right-radius: 0;
  border-bottom-right-radius: 0;
}
[dir=rtl] .mat-drawer {
  border-top-left-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-bottom-left-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-top-right-radius: 0;
  border-bottom-right-radius: 0;
  transform: translate3d(100%, 0, 0);
}
[dir=rtl] .mat-drawer.mat-drawer-end {
  border-top-right-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-bottom-right-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-top-left-radius: 0;
  border-bottom-left-radius: 0;
  left: 0;
  right: auto;
  transform: translate3d(-100%, 0, 0);
}
.mat-drawer-transition .mat-drawer {
  transition: transform 400ms cubic-bezier(0.25, 0.8, 0.25, 1);
}
.mat-drawer:not(.mat-drawer-opened):not(.mat-drawer-animating) {
  visibility: hidden;
  box-shadow: none;
}
.mat-drawer:not(.mat-drawer-opened):not(.mat-drawer-animating) .mat-drawer-inner-container {
  display: none;
}
.mat-drawer.mat-drawer-opened.mat-drawer-opened {
  transform: none;
}

.mat-drawer-side {
  box-shadow: none;
  border-right-color: var(--%NS%mat-sidenav-container-divider-color, transparent);
  border-right-width: 1px;
  border-right-style: solid;
}
.mat-drawer-side.mat-drawer-end {
  border-left-color: var(--%NS%mat-sidenav-container-divider-color, transparent);
  border-left-width: 1px;
  border-left-style: solid;
  border-right: none;
}
[dir=rtl] .mat-drawer-side {
  border-left-color: var(--%NS%mat-sidenav-container-divider-color, transparent);
  border-left-width: 1px;
  border-left-style: solid;
  border-right: none;
}
[dir=rtl] .mat-drawer-side.mat-drawer-end {
  border-right-color: var(--%NS%mat-sidenav-container-divider-color, transparent);
  border-right-width: 1px;
  border-right-style: solid;
  border-left: none;
}

.mat-drawer-inner-container {
  width: 100%;
  height: 100%;
  overflow: auto;
}

.mat-sidenav-fixed {
  position: fixed;
}
`],encapsulation:2})})()}return a})()});var ne;var Ye=w$1(()=>{wt();Xr();$M();$M();ne=class a{destroyRef=D(he$1);isBrowser=Ls(D(HI));mediaMatcher=D(H);matchers=/* @__PURE__ */ new Map;match(u){let e=this.matchers.get(u);if(e)return e.matches.asReadonly();let t=this.mediaMatcher.matchMedia(u),n=nt(t.matches),r=i=>{n.set(i.matches)};return this.isBrowser&&(t.addEventListener(`change`,r),this.destroyRef.onDestroy(()=>{t.removeEventListener(`change`,r)})),this.matchers.set(u,{matches:n,handler:r}),n.asReadonly()}static ɵfac=function(e){return new(e||a)};static ɵprov=oe({token:a,factory:a.ɵfac,providedIn:`root`})}});var Kt=w$1(()=>{Ye()});var Jt=w$1(()=>{Ye();Kt()});function On(a,u){if(a&1){let e=GT();Rs(0,`a`,20,0),iv(`click`,function(){lh(e);let n=e_(2);return uh(n.selectConversation())}),Rs(2,`span`,21),O_(3),ud()()}if(a&2){let e=u.$implicit,t=c_(1);Zy(`routerLink`,Z_(3,An,e.id)),Qy(`data-active`,t.isActive||null),sb(3),gd(` `,e.title,` `)}}function Bn(a,u){if(a&1&&(Rs(0,`div`,17),O_(1),ud(),Rs(2,`div`,18),HT(3,On,4,5,`a`,19,Pn),ud()),a&2){let e=u.$implicit;sb(),gd(` `,e.label,` `),sb(2),BT(e.conversations)}}function zn(a,u){if(a&1&&(Rs(0,`button`,32)(1,`div`,49),Yy(2,`mat-icon`,50),Rs(3,`span`,51),O_(4),ud()(),Rs(5,`div`,52),O_(6),ud()()),a&2){let e=u.$implicit;sb(2),Zy(`svgIcon`,e.icon),sb(2),gd(` `,e.title,` `),sb(2),gd(` `,e.description,` `)}}function Vn(a,u){if(a&1&&(Rs(0,`button`,45),O_(1),ud()),a&2){let e=u.$implicit;sb(),_v(e)}}function Ln(a,u){if(a&1){let e=GT();Rs(0,`div`,22)(1,`button`,23),iv(`click`,function(){lh(e);let n=e_();return uh(n.panelOpened.set(!0))}),Yy(2,`mat-icon`,24),ud()(),Rs(3,`div`,25)(4,`div`,26)(5,`div`,27),Yy(6,`mat-icon`,28),Rs(7,`div`,29),O_(8),X_(9,`transloco`),ud(),Rs(10,`div`,30),O_(11),X_(12,`transloco`),ud()(),Rs(13,`div`,31),HT(14,zn,7,3,`button`,32,Fn),ud()()(),Rs(16,`div`,33)(17,`div`,34)(18,`div`,35),Yy(19,`textarea`,36),X_(20,`transloco`),Rs(21,`div`,37)(22,`button`,8),Yy(23,`mat-icon`,38),ud(),Rs(24,`button`,8),Yy(25,`mat-icon`,39),ud(),Yy(26,`div`,40),Rs(27,`button`,41)(28,`span`,42),O_(29),Yy(30,`mat-icon`,43),ud()(),Rs(31,`mat-menu`,44,1),HT(33,Vn,2,1,`button`,45,VT),ud(),Rs(35,`button`,46),Yy(36,`mat-icon`,47),ud()()(),Rs(37,`div`,48),O_(38),X_(39,`transloco`),ud()()()}if(a&2){let e=c_(32),t=e_();sb(),Zy(`matTooltip`,`Conversations`),sb(7),gd(` `,tM(9,11,`ui.how_can_i_help_you_today`),` `),sb(3),gd(` `,tM(12,13,`ui.pick_a_suggestion_below_or_start_typing`),` `),sb(3),BT(t.suggestions),sb(5),Zy(`placeholder`,tM(20,15,`ui.message_ai_chat`))(`cdkAutosizeMinRows`,1),sb(3),Zy(`matTooltip`,`Attach a file`),sb(2),Zy(`matTooltip`,`Dictate`),sb(3),Zy(`matMenuTriggerFor`,e),sb(2),gd(` `,t.model,` `),sb(4),BT(t.models),sb(2),Zy(`matTooltip`,`Send`),sb(3),gd(` `,tM(39,17,`ui.ai_can_make_mistakes_check_important_info`),` `)}}var An;var Rn;var Pn;var Fn;var Ge;var jn=w$1(()=>{at();ae();$M();St$1();gt();Ee();Zt();qt();b();Ba();w();Jt();$M();An=a=>[`./`,a],Rn=(a,u)=>u.label,Pn=(a,u)=>u.id,Fn=(a,u)=>u.title;Ge=class a{aiChatService=D(h);media=D(ne);data=this.aiChatService.data;isMobile=ye(()=>this.media.match(`(max-width: 1023px)`)());model=`Nova 4.5`;models=[`Nova 4.5`,`Nova 4.5 Mini`,`Nova 3 Turbo`];suggestions=[{icon:`code`,title:`Review a migration plan`,description:`Walk through the risky steps before we ship it.`},{icon:`file-text`,title:`Summarize a long document`,description:`Pull out the decisions and the open questions.`},{icon:`lightbulb`,title:`Brainstorm launch angles`,description:`Three positioning options for a new plan.`},{icon:`bug`,title:`Debug a failing query`,description:`Explain a plan and suggest a better index.`}];groups=ye(()=>this.groupConversations());hasConversation=nt(!1);panelOpened=nt(!1);selectConversation(){this.isMobile()&&this.panelOpened.set(!1)}groupConversations(){let u=[...this.data.conversations].sort((i,o)=>new Date(o.createdAt).getTime()-new Date(i.createdAt).getTime()),e=i=>{let o=new Date(i);return new Date(o.getFullYear(),o.getMonth(),o.getDate()).getTime()},t=e(u[0].createdAt),n=i=>Math.round((t-e(i))/864e5),r=[{label:`Today`,conversations:[]},{label:`Yesterday`,conversations:[]},{label:`Previous 7 days`,conversations:[]}];for(let i of u){let o=n(i.createdAt);r[o===0?0:o===1?1:2].conversations.push(i)}return r.filter(i=>i.conversations.length>0)}static ɵfac=function(e){return new(e||a)};static ɵcmp=OC({type:a,selectors:[[`ai-chat`]],hostAttrs:[1,`lg:h-full`],decls:21,vars:12,consts:[[`rla`,`routerLinkActive`],[`modelMenu`,`matMenu`],[1,`@container`,`mx-auto`,`flex`,`h-full`,`w-full`,`flex-auto`,`flex-col`,`overflow-hidden`],[1,`h-full`,`flex-auto`,`[&_.mat-drawer-backdrop]:fixed`,3,`backdropClick`],[`disableClose`,``,1,`w-72`,`border-r`,`border-neutral-200`,`bg-neutral-50`,`dark:border-neutral-800`,`dark:bg-neutral-900`,3,`mode`,`opened`,`position`,`fixedInViewport`],[1,`flex`,`h-full`,`w-full`,`flex-col`],[1,`flex`,`items-center`,`gap-x-2`,`py-4`,`pr-3`,`pl-4`],[1,`flex-auto`,`text-lg`,`font-semibold`,`tracking-tighter`],[`matIconButton`,``,1,`text-neutral-500`,3,`matTooltip`],[`svgIcon`,`square-pen`],[1,`px-4`],[1,`w-full`],[`matPrefix`,``,`svgIcon`,`search`],[`matInput`,``,3,`placeholder`],[1,`mt-4`,`flex-auto`,`overflow-y-auto`,`px-2`,`pb-4`],[1,`flex`,`flex-auto`,`flex-col`,`overflow-hidden`],[3,`activate`,`deactivate`],[1,`px-2`,`pt-4`,`pb-1`,`text-xs`,`font-medium`,`text-neutral-500`,`uppercase`],[1,`flex`,`flex-col`,`gap-y-1`],[`routerLinkActive`,`bg-neutral-700/10 dark:bg-neutral-300/10`,1,`flex`,`items-center`,`rounded-lg`,`px-2`,`py-2`,`not-data-active:hover:bg-neutral-700/5`,`dark:not-data-active:hover:bg-neutral-300/5`,3,`routerLink`],[`routerLinkActive`,`bg-neutral-700/10 dark:bg-neutral-300/10`,1,`flex`,`items-center`,`rounded-lg`,`px-2`,`py-2`,`not-data-active:hover:bg-neutral-700/5`,`dark:not-data-active:hover:bg-neutral-300/5`,3,`click`,`routerLink`],[1,`min-w-0`,`flex-auto`,`truncate`],[1,`flex`,`items-center`,`gap-x-2`,`px-4`,`py-4`,`lg:hidden`],[`matIconButton`,``,1,`text-neutral-500`,3,`click`,`matTooltip`],[`svgIcon`,`menu`],[1,`flex`,`flex-auto`,`flex-col`,`items-center`,`justify-center`,`px-6`,`py-8`,`lg:px-8`],[1,`w-full`,`max-w-3xl`],[1,`flex`,`flex-col`,`items-center`,`text-center`],[`svgIcon`,`sparkles`,1,`size-10`,`text-primary-600`],[1,`mt-4`,`text-2xl`,`font-semibold`,`tracking-tighter`,`sm:text-3xl`],[1,`mt-1`,`text-neutral-500`],[1,`mt-8`,`grid`,`grid-cols-1`,`gap-3`,`@xl:grid-cols-2`],[`type`,`button`,1,`flex`,`cursor-pointer`,`flex-col`,`gap-y-1`,`rounded-xl`,`border`,`p-4`,`text-left`,`hover:bg-neutral-100`,`dark:hover:bg-white/2.5`],[1,`px-6`,`pb-6`,`lg:px-8`],[1,`mx-auto`,`w-full`,`max-w-3xl`],[1,`rounded-2xl`,`border`,`bg-white`,`p-2`,`dark:bg-neutral-800`],[`cdkTextareaAutosize`,``,`cdkAutosizeMaxRows`,`8`,1,`w-full`,`resize-none`,`border-0`,`bg-transparent`,`px-2`,`py-2`,`outline-none`,3,`placeholder`,`cdkAutosizeMinRows`],[1,`flex`,`items-center`,`gap-x-1`],[`svgIcon`,`paperclip`],[`svgIcon`,`mic`],[1,`flex-auto`],[`type`,`button`,1,`text-sm`,`font-medium`,`text-neutral-500`,3,`matMenuTriggerFor`],[1,`flex`,`items-center`,`gap-x-1`,`px-2`,`py-1`],[`svgIcon`,`chevron-down`,1,`size-4`],[`xPosition`,`before`],[`mat-menu-item`,``],[`matIconButton`,``,1,`bg-primary-600`,`text-white`,3,`matTooltip`],[`svgIcon`,`send`],[1,`mt-2`,`text-center`,`text-xs`,`text-neutral-400`],[1,`flex`,`items-center`,`gap-x-2`],[1,`size-4`,`shrink-0`,`text-neutral-400`,3,`svgIcon`],[1,`text-sm`,`font-medium`],[1,`text-sm`,`text-neutral-500`]],template:function(e,t){e&1&&(Rs(0,`div`,2)(1,`mat-sidenav-container`,3),iv(`backdropClick`,function(){return t.panelOpened.set(!1)}),Rs(2,`mat-sidenav`,4)(3,`div`,5)(4,`div`,6)(5,`div`,7),O_(6),X_(7,`transloco`),ud(),Rs(8,`button`,8),Yy(9,`mat-icon`,9),ud()(),Rs(10,`div`,10)(11,`mat-form-field`,11),Yy(12,`mat-icon`,12)(13,`input`,13),X_(14,`transloco`),ud()(),Rs(15,`div`,14),HT(16,Bn,5,1,null,null,Rn),ud()()(),Rs(18,`mat-sidenav-content`,15)(19,`router-outlet`,16),iv(`activate`,function(){return t.hasConversation.set(!0)})(`deactivate`,function(){return t.hasConversation.set(!1)}),ud(),LT(20,Ln,40,19),ud()()()),e&2&&(sb(2),Zy(`mode`,t.isMobile()?`over`:`side`)(`opened`,!t.isMobile()||t.panelOpened())(`position`,`start`)(`fixedInViewport`,t.isMobile()),sb(4),gd(` `,tM(7,8,`ui.ai_chat`),` `),sb(2),Zy(`matTooltip`,`New chat`),sb(5),Zy(`placeholder`,tM(14,10,`ui.search_chats`)),sb(3),BT(t.groups()),sb(4),FT(t.hasConversation()?-1:20))},dependencies:[Oe,zt,gt$1,rt,yt,Mt$1,te,pe,Ut,Qe,$t,Ce,Vt,Yt,Ta,Pr,Nt],encapsulation:2})}});export{pe as a,jn as i,Ut as n,te as o,Zt as r,Ge as t};