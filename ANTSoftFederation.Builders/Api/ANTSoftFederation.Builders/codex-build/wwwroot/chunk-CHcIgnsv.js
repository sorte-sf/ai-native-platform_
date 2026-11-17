import{$t as R$1,Ft as OC,Hi as xo,J as HB,Mn as X,Qr as nt,Rr as lt,ir as cr,jt as Ne,k as D,l as $p,li as re,o as $M,pt as LC,vt as Lo}from"./chunk-awiZbSKC.js";import{o as w}from"./chunk-Bhnl8H4w.js";var d;var L;var x=w(()=>{$M();$M();d=/* @__PURE__ */ new WeakMap,L=(()=>{class e{_appRef;_injector=D(re);_environmentInjector=D(Ne);load(n){let t=this._appRef=this._appRef||this._injector.get(Lo),i=d.get(t);i||(i={loaders:/* @__PURE__ */ new Set,refs:[]},d.set(t,i),t.onDestroy(()=>{d.get(t)?.refs.forEach(I=>I.destroy()),d.delete(t)})),i.loaders.has(n)||(i.loaders.add(n),i.refs.push(HB(n,{environmentInjector:this._environmentInjector})))}static ɵfac=function(t){return new(t||e)};static ɵprov=xo({token:e,factory:e.ɵfac})}return e})()});function T(){if(c===void 0&&(c=null,typeof window<`u`)){let e=window;if(e.trustedTypes!==void 0)try{c=e.trustedTypes.createPolicy(`angular#components`,{createHTML:r=>r})}catch(r){console.error(r)}}return c}function b(e){return T()?.createHTML(e)||e}function W(e,r,n){e.innerHTML=b(n.sanitize(X.HTML,r)||``)}var U;var c;var H=w(()=>{x();$M();$M();U=(()=>{class e{static ɵfac=function(t){return new(t||e)};static ɵcmp=OC({type:e,selectors:[[`ng-component`]],exportAs:[`cdkVisuallyHidden`],decls:0,vars:0,template:function(t,i){},styles:[`.cdk-visually-hidden {
  border: 0;
  clip: rect(0 0 0 0);
  height: 1px;
  margin: -1px;
  overflow: hidden;
  padding: 0;
  position: absolute;
  width: 1px;
  white-space: nowrap;
  outline: 0;
  -webkit-appearance: none;
  -moz-appearance: none;
  left: 0;
}
[dir=rtl] .cdk-visually-hidden {
  left: auto;
  right: 0;
}
`],encapsulation:2})}return e})()});function E(e){let r=e?.toLowerCase()||``;return r===`auto`&&typeof navigator<`u`&&navigator?.language?N.test(navigator.language)?`rtl`:`ltr`:r===`rtl`?`rtl`:`ltr`}var j;var N;var A;var k=w(()=>{$M();$M();j=new R$1(`cdk-dir-doc`,{providedIn:`root`,factory:()=>D(cr)}),N=/^(ar|ckb|dv|he|iw|fa|nqo|ps|sd|ug|ur|yi|.*[-_](Adlm|Arab|Hebr|Nkoo|Rohg|Thaa))(?!.*[-_](Latn|Cyrl)($|-|_))($|-|_)/i;A=(()=>{class e{get value(){return this.valueSignal()}valueSignal=nt(`ltr`);change=new lt;constructor(){let n=D(j,{optional:!0});if(n){let t=n.body?n.body.dir:null,i=n.documentElement?n.documentElement.dir:null;this.valueSignal.set(E(t||i||`ltr`))}}ngOnDestroy(){this.change.complete()}static ɵfac=function(t){return new(t||e)};static ɵprov=xo({token:e,factory:e.ɵfac})}return e})()});var ee;var R=w(()=>{$M();k();ee=(()=>{class e{static ɵfac=function(t){return new(t||e)};static ɵmod=LC({type:e});static ɵinj=$p({})}return e})()});export{U as a,ee as c,R as i,k as l,H as n,W as o,L as r,b as s,A as t,x as u};