import{i as Xe$1,r as We$1}from"./chunk-DWARYugZ.js";import{Er as jC,Ft as OC,Gi as ye,Li as xB,Qn as b_,Qt as Qy,a as $I,ci as r_,ei as oe$1,ir as cr$1,k as D$1,ki as vw,l as $p,ni as os,o as $M,on as SB,pt as LC,qr as n_,ui as rv}from"./chunk-awiZbSKC.js";import{n as s,o as w,t as r}from"./chunk-Bhnl8H4w.js";function Ae(e,t){(t==null||t>e.length)&&(t=e.length);for(var n=0,a=Array(t);n<t;n++)a[n]=e[n];return a}function ma(e){if(Array.isArray(e))return e}function ga(e){if(Array.isArray(e))return Ae(e)}function pa(e,t){if(!(e instanceof t))throw new TypeError(`Cannot call a class as a function`)}function bn(e,t){for(var n=0;n<t.length;n++){var a=t[n];a.enumerable=a.enumerable||!1,a.configurable=!0,`value`in a&&(a.writable=!0),Object.defineProperty(e,Kn(a.key),a)}}function va(e,t,n){return t&&bn(e.prototype,t),n&&bn(e,n),Object.defineProperty(e,"prototype",{writable:!1}),e}function re(e,t){var n=typeof Symbol<`u`&&e[Symbol.iterator]||e[`@@iterator`];if(!n){if(Array.isArray(e)||(n=We(e))||t&&e&&typeof e.length==`number`){n&&(e=n);var a=0,i=function(){};return{s:i,n:function(){return a>=e.length?{done:!0}:{done:!1,value:e[a++]}},e:function(l){throw l},f:i}}throw new TypeError(`Invalid attempt to iterate non-iterable instance.
In order to be iterable, non-array objects must have a [Symbol.iterator]() method.`)}var r,o=!0,s=!1;return{s:function(){n=n.call(e)},n:function(){var l=n.next();return o=l.done,l},e:function(l){s=!0,r=l},f:function(){try{o||n.return==null||n.return()}finally{if(s)throw r}}}}function g(e,t,n){return(t=Kn(t))in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function ha(e){if(typeof Symbol<`u`&&e[Symbol.iterator]!=null||e[`@@iterator`]!=null)return Array.from(e)}function ba(e,t){var n=e==null?null:typeof Symbol<`u`&&e[Symbol.iterator]||e[`@@iterator`];if(n!=null){var a,i,r,o,s=[],l=!0,u=!1;try{if(r=(n=n.call(e)).next,t===0){if(Object(n)!==n)return;l=!1}else for(;!(l=(a=r.call(n)).done)&&(s.push(a.value),s.length!==t);l=!0);}catch(d){u=!0,i=d}finally{try{if(!l&&n.return!=null&&(o=n.return(),Object(o)!==o))return}finally{if(u)throw i}}return s}}function ya(){throw new TypeError(`Invalid attempt to destructure non-iterable instance.
In order to be iterable, non-array objects must have a [Symbol.iterator]() method.`)}function xa(){throw new TypeError(`Invalid attempt to spread non-iterable instance.
In order to be iterable, non-array objects must have a [Symbol.iterator]() method.`)}function yn(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var a=Object.getOwnPropertySymbols(e);t&&(a=a.filter(function(i){return Object.getOwnPropertyDescriptor(e,i).enumerable})),n.push.apply(n,a)}return n}function f(e){for(var t=1;t<arguments.length;t++){var n=arguments[t]!=null?arguments[t]:{};t%2?yn(Object(n),!0).forEach(function(a){g(e,a,n[a])}):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):yn(Object(n)).forEach(function(a){Object.defineProperty(e,a,Object.getOwnPropertyDescriptor(n,a))})}return e}function ce(e,t){return ma(e)||ba(e,t)||We(e,t)||ya()}function F(e){return ga(e)||ha(e)||We(e)||xa()}function Sa(e,t){if(typeof e!=`object`||!e)return e;var n=e[Symbol.toPrimitive];if(n!==void 0){var a=n.call(e,t||`default`);if(typeof a!=`object`)return a;throw new TypeError(`@@toPrimitive must return a primitive value.`)}return(t===`string`?String:Number)(e)}function Kn(e){var t=Sa(e,`string`);return typeof t==`symbol`?t:t+``}function le(e){"@babel/helpers - typeof";return le=typeof Symbol==`function`&&typeof Symbol.iterator==`symbol`?function(t){return typeof t}:function(t){return t&&typeof Symbol==`function`&&t.constructor===Symbol&&t!==Symbol.prototype?`symbol`:typeof t},le(e)}function We(e,t){if(e){if(typeof e==`string`)return Ae(e,t);var n={}.toString.call(e).slice(8,-1);return n===`Object`&&e.constructor&&(n=e.constructor.name),n===`Map`||n===`Set`?Array.from(e):n===`Arguments`||/^(?:Ui|I)nt(?:8|16|32)(?:Clamped)?Array$/.test(n)?Ae(e,t):void 0}}function Q(e){return new Proxy(e,{get:function(n,a){return a in n?n[a]:n[S]}})}function kr(e){var t=h.querySelector(`script[`+e+`]`);if(t)return t.getAttribute(e)}function Cr(e){return e===``?!0:e===`false`?!1:e===`true`?!0:e}function Ar(e){return G.push(e),function(){G.splice(G.indexOf(e),1)}}function zr(e){if(!(!e||!M)){var t=h.createElement(`style`);t.setAttribute(`type`,`text/css`),t.innerHTML=e;for(var n=h.head.childNodes,a=null,i=n.length-1;i>-1;i--){var r=n[i],o=(r.tagName||``).toUpperCase();[`STYLE`,`LINK`].indexOf(o)>-1&&(a=r)}return h.head.insertBefore(t,a),e}}function Pn(){for(var e=12,t=``;e-->0;)t+=Fr[Math.random()*62|0];return t}function X(e){for(var t=[],n=(e||[]).length>>>0;n--;)t[n]=e[n];return t}function Xe(e){return e.classList?X(e.classList):(e.getAttribute(`class`)||``).split(` `).filter(function(t){return t})}function jt(e){return``.concat(e).replace(/&/g,`&amp;`).replace(/"/g,`&quot;`).replace(/'/g,`&#39;`).replace(/</g,`&lt;`).replace(/>/g,`&gt;`)}function Pr(e){return Object.keys(e||{}).reduce(function(t,n){return t+``.concat(n,`="`).concat(jt(e[n]),`" `)},``).trim()}function de(e){return Object.keys(e||{}).reduce(function(t,n){return t+``.concat(n,`: `).concat(e[n].trim(),`;`)},``)}function Ve(e){return e.size!==P.size||e.x!==P.x||e.y!==P.y||e.rotate!==P.rotate||e.flipX||e.flipY}function Nr(e){var t=e.transform,n=e.containerWidth,a=e.iconWidth,i={transform:`translate(`.concat(n/2,` 256)`)},r=`translate(`.concat(t.x*32,`, `).concat(t.y*32,`) `),o=`scale(`.concat(t.size/16*(t.flipX?-1:1),`, `).concat(t.size/16*(t.flipY?-1:1),`) `),s=`rotate(`.concat(t.rotate,` 0 0)`);return{outer:i,inner:{transform:``.concat(r,` `).concat(o,` `).concat(s)},path:{transform:`translate(`.concat(a/2*-1,` -256)`)}}}function Or(e){var t=e.transform,n=e.width,a=n===void 0?Fe:n,i=e.height,r=i===void 0?Fe:i,o=e.startCentered,s=o===void 0?!1:o,l=``;return s&&et?l+=`translate(`.concat(t.x/T-a/2,`em, `).concat(t.y/T-r/2,`em) `):s?l+=`translate(calc(-50% + `.concat(t.x/T,`em), calc(-50% + `).concat(t.y/T,`em)) `):l+=`translate(`.concat(t.x/T,`em, `).concat(t.y/T,`em) `),l+=`scale(`.concat(t.size/T*(t.flipX?-1:1),`, `).concat(t.size/T*(t.flipY?-1:1),`) `),l+=`rotate(`.concat(t.rotate,`deg) `),l}function Lt(){var e=Pt,t=Nt,n=m.cssPrefix,a=m.replacementClass,i=Dr;if(n!==e||a!==t){var r=new RegExp(`\\.`.concat(e,`\\-`),`g`),o=new RegExp(`\\--`.concat(e,`\\-`),`g`),s=new RegExp(`\\.`.concat(t),`g`);i=i.replace(r,`.`.concat(n,`-`)).replace(o,`--`.concat(n,`-`)).replace(s,`.`.concat(a))}return i}function we(){m.autoAddCss&&!Nn&&(zr(Lt()),Nn=!0)}function Er(e){M&&(fe?setTimeout(e,0):_t.push(e))}function Z(e){var t=e.tag,n=e.attributes,a=n===void 0?{}:n,i=e.children,r=i===void 0?[]:i;return typeof e==`string`?jt(e):`<`.concat(t,` `).concat(Pr(a),`>`).concat(r.map(Z).join(``),`</`).concat(t,`>`)}function On(e,t,n){if(e&&e[t]&&e[t][n])return{prefix:t,iconName:n,icon:e[t][n]}}function Rt(e){return F(e).length!==1?null:e.codePointAt(0).toString(16)}function Dn(e){return Object.keys(e).reduce(function(t,n){var a=e[n];return!!a.icon?t[a.iconName]=a.icon:t[n]=a,t},{})}function Me(e,t){var a=(arguments.length>2&&arguments[2]!==void 0?arguments[2]:{}).skipHooks,i=a===void 0?!1:a,r=Dn(t);typeof z.hooks.addPack==`function`&&!i?z.hooks.addPack(e,Dn(t)):z.styles[e]=f(f({},z.styles[e]||{}),r),e===`fas`&&Me(`fa`,t)}function _r(e){return~Ir.indexOf(e)}function $r(e,t){var n=t.split(`-`),a=n[0],i=n.slice(1).join(`-`);return a===e&&i!==``&&!_r(i)?i:null}function Ge(e,t){return(Ht[e]||{})[t]}function Rr(e,t){return(qt[e]||{})[t]}function $(e,t){return(Xt[e]||{})[t]}function Bt(e){return Ut[e]||{prefix:null,iconName:null}}function Wr(e){var t=Yt[e],n=Ge(`fas`,e);return t||(n?{prefix:`fas`,iconName:n}:null)||{prefix:null,iconName:null}}function L(){return Be}function Hr(e){var t=S,n=Wt.reduce(function(a,i){return a[i]=``.concat(m.cssPrefix,`-`).concat(i),a},{});return Ct.forEach(function(a){(e.includes(n[a])||e.some(function(i){return Lr[a].includes(i)}))&&(t=a)}),t}function me(e){var n=(arguments.length>1&&arguments[1]!==void 0?arguments[1]:{}).family,a=n===void 0?S:n,i=br[a][e];if(a===J&&!e)return`fad`;var r=zn[a][e]||zn[a][i],o=e in z.styles?e:null;return r||o||null}function qr(e){var t=[],n=null;return e.forEach(function(a){var i=$r(m.cssPrefix,a);i?n=i:a&&t.push(a)}),{iconName:n,rest:t}}function Mn(e){return e.sort().filter(function(t,n,a){return a.indexOf(t)===n})}function ge(e){var n=(arguments.length>1&&arguments[1]!==void 0?arguments[1]:{}).skipLookups,a=n===void 0?!1:n,i=null,r=Mn(e.filter(function(p){return En.includes(p)})),o=Mn(e.filter(function(p){return!En.includes(p)})),u=ce(r.filter(function(p){return i=p,!tt.includes(p)}),1)[0],d=u===void 0?null:u,c=Hr(r),v=f(f({},qr(o)),{},{prefix:me(d,{family:c})});return f(f(f({},v),Vr({values:e,family:c,styles:K,config:m,canonical:v,givenPrefix:i})),Ur(a,i,v))}function Ur(e,t,n){var a=n.prefix,i=n.iconName;if(e||!a||!i)return{prefix:a,iconName:i};var r=t===`fa`?Bt(i):{},o=$(a,i);return i=r.iconName||o||i,a=r.prefix||a,a===`far`&&!K.far&&K.fas&&!m.autoFetchSvg&&(a=`fas`),{prefix:a,iconName:i}}function Vr(e){var t=e.values,n=e.family,a=e.canonical,i=e.givenPrefix,r=i===void 0?``:i,o=e.styles,s=o===void 0?{}:o,l=e.config,u=l===void 0?{}:l,d=n===J,c=t.includes(`fa-duotone`)||t.includes(`fad`),v=u.familyDefault===`duotone`,p=a.prefix===`fad`||a.prefix===`fa-duotone`;if(!d&&(c||v||p)&&(a.prefix=`fad`),(t.includes(`fa-brands`)||t.includes(`fab`))&&(a.prefix=`fab`),!a.prefix&&Yr.includes(n)){if(Object.keys(s).find(function(w){return Xr.includes(w)})||u.autoFetchSvg)a.prefix=Qa.get(n).defaultShortPrefixId,a.iconName=$(a.prefix,a.iconName)||a.iconName}return(a.prefix===`fa`||r===`fa`)&&(a.prefix=L()||`fas`),a}function Kr(e,t){var n=t.mixoutsTo;return Tn=e,q={},Object.keys(U).forEach(function(a){Gr.indexOf(a)===-1&&delete U[a]}),Tn.forEach(function(a){var i=a.mixout?a.mixout():{};if(Object.keys(i).forEach(function(o){typeof i[o]==`function`&&(n[o]=i[o]),le(i[o])===`object`&&Object.keys(i[o]).forEach(function(s){n[o]||(n[o]={}),n[o][s]=i[o][s]})}),a.hooks){var r=a.hooks();Object.keys(r).forEach(function(o){q[o]||(q[o]=[]),q[o].push(r[o])})}a.provides&&a.provides(U)}),n}function Ee(e,t){for(var n=arguments.length,a=new Array(n>2?n-2:0),i=2;i<n;i++)a[i-2]=arguments[i];return(q[e]||[]).forEach(function(o){t=o.apply(null,[t].concat(a))}),t}function W(e){for(var t=arguments.length,n=new Array(t>1?t-1:0),a=1;a<t;a++)n[a-1]=arguments[a];(q[e]||[]).forEach(function(r){r.apply(null,n)})}function _(){var e=arguments[0],t=Array.prototype.slice.call(arguments,1);return U[e]?U[e].apply(null,t):void 0}function Te(e){e.prefix===`fa`&&(e.prefix=`fas`);var t=e.iconName,n=e.prefix||L();if(t)return t=$(n,t)||t,On(Kt.definitions,n,t)||On(z.styles,n,t)}function pe(e,t){return Object.defineProperty(e,"abstract",{get:t}),Object.defineProperty(e,"html",{get:function(){return e.abstract.map(function(a){return Z(a)})}}),Object.defineProperty(e,"node",{get:function(){if(M){var a=h.createElement(`div`);return a.innerHTML=e.html,a.children}}}),e}function no(e){var t=e.children,n=e.main,a=e.mask,i=e.attributes,r=e.styles,o=e.transform;if(Ve(o)&&n.found&&!a.found){var u={x:n.width/n.height/2,y:.5};i.style=de(f(f({},r),{},{"transform-origin":``.concat(u.x+o.x/16,`em `).concat(u.y+o.y/16,`em`)}))}return[{tag:`svg`,attributes:i,children:t}]}function to(e){var t=e.prefix,n=e.iconName,a=e.children,i=e.attributes,r=e.symbol,o=r===!0?``.concat(t,`-`).concat(m.cssPrefix,`-`).concat(n):r;return[{tag:`svg`,attributes:{style:`display: none;`},children:[{tag:`symbol`,attributes:f(f({},i),{},{id:o}),children:a}]}]}function ao(e){return[`aria-label`,`aria-labelledby`,`title`,`role`].some(function(n){return n in e})}function Ke(e){var t=e.icons,n=t.main,a=t.mask,i=e.prefix,r=e.iconName,o=e.transform,s=e.symbol,l=e.maskId,u=e.extra,d=e.watchable,c=d===void 0?!1:d,v=a.found?a:n,p=v.width,y=v.height,b=[m.replacementClass,r?``.concat(m.cssPrefix,`-`).concat(r):``].filter(function(N){return u.classes.indexOf(N)===-1}).filter(function(N){return N!==``||!!N}).concat(u.classes).join(` `),w={children:[],attributes:f(f({},u.attributes),{},{"data-prefix":i,"data-icon":r,class:b,role:u.attributes.role||`img`,viewBox:`0 0 `.concat(p,` `).concat(y)})};!ao(u.attributes)&&!u.attributes[`aria-hidden`]&&(w.attributes[`aria-hidden`]=`true`),c&&(w.attributes[R]=``);var x=f(f({},w),{},{prefix:i,iconName:r,main:n,mask:a,maskId:l,transform:o,symbol:s,styles:f({},u.styles)}),k=a.found&&n.found?_(`generateAbstractMask`,x)||{children:[],attributes:{}}:_(`generateAbstractIcon`,x)||{children:[],attributes:{}},A=k.children,H=k.attributes;return x.children=A,x.attributes=H,s?to(x):no(x)}function jn(e){var t=e.content,n=e.width,a=e.height,i=e.transform,r=e.extra,o=e.watchable,s=o===void 0?!1:o,l=f(f({},r.attributes),{},{class:r.classes.join(` `)});s&&(l[R]=``);var u=f({},r.styles);Ve(i)&&(u.transform=Or({transform:i,startCentered:!0,width:n,height:a}),u[`-webkit-transform`]=u.transform);var d=de(u);d.length>0&&(l.style=d);var c=[];return c.push({tag:`span`,attributes:l,children:[t]}),c}function io(e){var t=e.content,n=e.extra,a=f(f({},n.attributes),{},{class:n.classes.join(` `)}),i=de(n.styles);i.length>0&&(a.style=i);var r=[];return r.push({tag:`span`,attributes:a,children:[t]}),r}function je(e){var t=e[0],n=e[1],r=ce(e.slice(4),1)[0],o=null;return Array.isArray(r)?o={tag:`g`,attributes:{class:``.concat(m.cssPrefix,`-`).concat(Se.GROUP)},children:[{tag:`path`,attributes:{class:``.concat(m.cssPrefix,`-`).concat(Se.SECONDARY),fill:`currentColor`,d:r[0]}},{tag:`path`,attributes:{class:``.concat(m.cssPrefix,`-`).concat(Se.PRIMARY),fill:`currentColor`,d:r[1]}}]}:o={tag:`path`,attributes:{fill:`currentColor`,d:r}},{found:!0,width:t,height:n,icon:o}}function oo(e,t){!Dt&&!m.showMissingIcons&&e&&console.error(`Icon with name "`.concat(e,`" and prefix "`).concat(t,`" is missing.`))}function Le(e,t){var n=t;return t===`fa`&&m.styleDefault!==null&&(t=L()),new Promise(function(a,i){if(n===`fa`){var r=Bt(e)||{};e=r.iconName||e,t=r.prefix||t}if(e&&t&&ke[t]&&ke[t][e]){var o=ke[t][e];return a(je(o))}oo(e,t),a(f(f({},ro),{},{icon:m.showMissingIcons&&e?_(`missingIconAbstract`)||{}:{}}))})}function _n(e){return typeof(e.getAttribute?e.getAttribute(R):null)==`string`}function lo(e){var t=e.getAttribute?e.getAttribute(qe):null,n=e.getAttribute?e.getAttribute(Ue):null;return t&&n}function fo(e){return e&&e.classList&&e.classList.contains&&e.classList.contains(m.replacementClass)}function uo(){if(m.autoReplaceSvg===!0)return se.replace;return se[m.autoReplaceSvg]||se.replace}function co(e){return h.createElementNS(`http://www.w3.org/2000/svg`,e)}function mo(e){return h.createElement(e)}function Qt(e){var n=(arguments.length>1&&arguments[1]!==void 0?arguments[1]:{}).ceFn,a=n===void 0?e.tag===`svg`?co:mo:n;if(typeof e==`string`)return h.createTextNode(e);var i=a(e.tag);Object.keys(e.attributes||[]).forEach(function(o){i.setAttribute(o,e.attributes[o])});return(e.children||[]).forEach(function(o){i.appendChild(Qt(o,{ceFn:a}))}),i}function go(e){var t=` `.concat(e.outerHTML,` `);return t=``.concat(t,`Font Awesome fontawesome.com `),t}function $n(e){e()}function Zt(e,t){var n=typeof t==`function`?t:oe;if(e.length===0)n();else{var a=$n;m.mutateApproach===vr&&(a=j.requestAnimationFrame||$n),a(function(){var i=uo(),r=Je.begin(`mutate`);e.map(i),r(),n()})}}function ea(){Qe=!0}function $e(){Qe=!1}function Rn(e){if(In&&m.observeMutations){var t=e.treeCallback,n=t===void 0?oe:t,a=e.nodeCallback,i=a===void 0?oe:a,r=e.pseudoElementsCallback,o=r===void 0?oe:r,s=e.observeMutationsRoot,l=s===void 0?h:s;ue=new In(function(u){if(!Qe){var d=L();X(u).forEach(function(c){if(c.type===`childList`&&c.addedNodes.length>0&&!_n(c.addedNodes[0])&&(m.searchPseudoElements&&o(c.target),n(c.target)),c.type===`attributes`&&c.target.parentNode&&m.searchPseudoElements&&o([c.target],!0),c.type===`attributes`&&_n(c.target)&&~wr.indexOf(c.attributeName))if(c.attributeName===`class`&&lo(c.target)){var v=ge(Xe(c.target)),p=v.prefix,y=v.iconName;c.target.setAttribute(qe,p||d),y&&c.target.setAttribute(Ue,y)}else fo(c.target)&&i(c.target)})}}),M&&ue.observe(l,{childList:!0,attributes:!0,characterData:!0,subtree:!0})}}function po(){ue&&ue.disconnect()}function vo(e){var t=e.getAttribute(`style`),n=[];return t&&(n=t.split(`;`).reduce(function(a,i){var r=i.split(`:`),o=r[0],s=r.slice(1);return o&&s.length>0&&(a[o]=s.join(`:`).trim()),a},{})),n}function ho(e){var t=e.getAttribute(`data-prefix`),n=e.getAttribute(`data-icon`),a=e.innerText!==void 0?e.innerText.trim():``,i=ge(Xe(e));return i.prefix||(i.prefix=L()),t&&n&&(i.prefix=t,i.iconName=n),i.iconName&&i.prefix||(i.prefix&&a.length>0&&(i.iconName=Rr(i.prefix,e.innerText)||Ge(i.prefix,Rt(e.innerText))),!i.iconName&&m.autoFetchSvg&&e.firstChild&&e.firstChild.nodeType===Node.TEXT_NODE&&(i.iconName=e.firstChild.data)),i}function bo(e){return X(e.attributes).reduce(function(n,a){return n.name!==`class`&&n.name!==`style`&&(n[a.name]=a.value),n},{})}function yo(){return{iconName:null,prefix:null,transform:P,symbol:!1,mask:{iconName:null,prefix:null,rest:[]},maskId:null,extra:{classes:[],styles:{},attributes:{}}}}function Wn(e){var t=arguments.length>1&&arguments[1]!==void 0?arguments[1]:{styleParser:!0},n=ho(e),a=n.iconName,i=n.prefix,r=n.rest,o=bo(e),s=Ee(`parseNodeAttributes`,{},e),l=t.styleParser?vo(e):[];return f({iconName:a,prefix:i,transform:P,mask:{iconName:null,prefix:null,rest:[]},maskId:null,symbol:!1,extra:{classes:r,styles:l,attributes:o}},s)}function na(e){var t=m.autoReplaceSvg===`nest`?Wn(e,{styleParser:!1}):Wn(e);return~t.extra.classes.indexOf(Et)?_(`generateLayersText`,e,t):_(`generateSvgReplacementMutation`,e,t)}function So(){return[].concat(F(At),F(zt))}function Hn(e){var t=arguments.length>1&&arguments[1]!==void 0?arguments[1]:null;if(!M)return Promise.resolve();var n=h.documentElement.classList,a=function(c){return n.add(``.concat(An,`-`).concat(c))},i=function(c){return n.remove(``.concat(An,`-`).concat(c))},r=m.autoFetchSvg?So():tt.concat(Object.keys(xo));r.includes(`fa`)||r.push(`fa`);var o=[`.`.concat(Et,`:not([`).concat(R,`])`)].concat(r.map(function(d){return`.`.concat(d,`:not([`).concat(R,`])`)})).join(`, `);if(o.length===0)return Promise.resolve();var s=[];try{s=X(e.querySelectorAll(o))}catch{}if(s.length>0)a(`pending`),i(`complete`);else return Promise.resolve();var l=Je.begin(`onTree`),u=s.reduce(function(d,c){try{var v=na(c);v&&d.push(v)}catch(p){Dt||p.name===`MissingIcon`&&console.error(p)}return d},[]);return new Promise(function(d,c){Promise.all(u).then(function(v){Zt(v,function(){a(`active`),a(`complete`),i(`pending`),typeof t==`function`&&t(),l(),d()})}).catch(function(v){l(),c(v)})})}function wo(e){var t=arguments.length>1&&arguments[1]!==void 0?arguments[1]:null;na(e).then(function(n){n&&Zt([n],t)})}function Io(e){return function(t){var n=arguments.length>1&&arguments[1]!==void 0?arguments[1]:{},a=(t||{}).icon?t:Te(t||{}),i=n.mask;return i&&(i=(i||{}).icon?i:Te(i||{})),e(a,f(f({},n),{},{mask:i}))}}function No(e){return Rt(F(e.replace(ta,``))[0]||``)}function Oo(e){var t=e.getPropertyValue(`font-feature-settings`).includes(`ss01`),a=e.getPropertyValue(`content`).replace(ta,``),i=a.codePointAt(0),r=i>=qn[0]&&i<=qn[1],o=a.length===2?a[0]===a[1]:!1;return r||o||t}function Do(e,t){var n=e.replace(/^['"]|['"]$/g,``).toLowerCase(),a=parseInt(t),i=isNaN(a)?`normal`:a;return(Re[n]||{})[i]||Po[n]}function Yn(e,t){var n=``.concat(pr).concat(t.replace(`:`,`-`));return new Promise(function(a,i){if(e.getAttribute(n)!==null)return a();var o=X(e.children).filter(function(ve){return ve.getAttribute(Pe)===t})[0],s=j.getComputedStyle(e,t),l=s.getPropertyValue(`font-family`),u=l.match(xr),d=s.getPropertyValue(`font-weight`),c=s.getPropertyValue(`content`);if(o&&!u)return e.removeChild(o),a();if(u&&c!==`none`&&c!==``){var v=s.getPropertyValue(`content`),p=Do(l,d),y=No(v),b=u[0].startsWith(`FontAwesome`),w=Oo(s),x=Ge(p,y),k=x;if(b){var A=Wr(y);A.iconName&&A.prefix&&(x=A.iconName,p=A.prefix)}if(x&&!w&&(!o||o.getAttribute(qe)!==p||o.getAttribute(Ue)!==k)){e.setAttribute(n,k),o&&e.removeChild(o);var H=yo(),N=H.extra;N.attributes[Pe]=t,Le(x,p).then(function(ve){var ua=Ke(f(f({},H),{},{icons:{main:ve,mask:Gt()},prefix:p,iconName:k,extra:N,watchable:!0})),he=h.createElementNS(`http://www.w3.org/2000/svg`,`svg`);t===`::before`?e.insertBefore(he,e.firstChild):e.appendChild(he),he.outerHTML=ua.map(function(ca){return Z(ca)}).join(`
`),e.removeAttribute(n),a()}).catch(i)}else a()}else a()})}function Mo(e){return Promise.all([Yn(e,`::before`),Yn(e,`::after`)])}function Eo(e){return e.parentNode!==document.head&&!~hr.indexOf(e.tagName.toUpperCase())&&!e.getAttribute(Pe)&&(!e.parentNode||e.parentNode.tagName!==`svg`)}function Xn(e){var t=arguments.length>1&&arguments[1]!==void 0?arguments[1]:!1;if(M){var n;if(t)n=e;else if(m.searchPseudoElementsFullScan)n=e.querySelectorAll(`*`);else{var a=/* @__PURE__ */ new Set,i=re(document.styleSheets),r;try{for(i.s();!(r=i.n()).done;){var o=r.value;try{var s=re(o.cssRules),l;try{for(s.s();!(l=s.n()).done;){var u=l.value,c=re(jo(u.selectorText)),v;try{for(c.s();!(v=c.n()).done;){var p=v.value;a.add(p)}}catch(b){c.e(b)}finally{c.f()}}}catch(b){s.e(b)}finally{s.f()}}catch(b){m.searchPseudoElementsWarnings&&console.warn(`Font Awesome: cannot parse stylesheet: `.concat(o.href,` (`).concat(b.message,`)
If it declares any Font Awesome CSS pseudo-elements, they will not be rendered as SVG icons. Add crossorigin="anonymous" to the <link>, enable searchPseudoElementsFullScan for slower but more thorough DOM parsing, or suppress this warning by setting searchPseudoElementsWarnings to false.`))}}}catch(b){i.e(b)}finally{i.f()}if(!a.size)return;var y=Array.from(a).join(`, `);try{n=e.querySelectorAll(y)}catch{}}return new Promise(function(b,w){var x=X(n).filter(Eo).map(Mo),k=Je.begin(`searchPseudoElements`);ea(),Promise.all(x).then(function(){k(),$e(),b()}).catch(function(){k(),$e(),w()})})}}function Gn(e){var t=arguments.length>1&&arguments[1]!==void 0?arguments[1]:!0;return e.attributes&&(e.attributes.fill||t)&&(e.attributes.fill=`black`),e}function Ro(e){return e.tag===`g`?e.children:[e]}var xn;var He;var Jn;var Qn;var Zn;var wa;var Sn;var wn;var j;var h;var In;var ne;var M;var et;var te;var Ia;var ka;var nt;var Ca;var tt;var S;var J;var at;var it;var rt;var ot;var st;var lt;var ft;var ut;var ct;var dt;var mt;var gt;var pt;var vt;var ht;var bt;var yt;var xt;var St;var wt;var It;var kt;var Aa;var za;var Fa;var Pa;var Na;var Oa;var Da;var Ma;var Ea;var Ta;var ja;var La;var _a;var $a;var Ra;var Wa;var Ha;var qa;var Ua;var Ya;var Xa;var Va;var Ba;var Ga;var Ct;var Ka;var Ja;var Qa;var Za;var At;var kn;var ei;var ni;var ti;var ai;var ii;var ri;var oi;var si;var Cn;var ae;var ie;var li;var fi;var ui;var ci;var di;var mi;var gi;var pi;var vi;var hi;var bi;var yi;var xi;var Si;var wi;var Ii;var ki;var Ci;var Ai;var zi;var Fi;var Pi;var Ni;var Oi;var Di;var Mi;var Ei;var Ti;var ji;var Li;var _i;var $i;var Ri;var Wi;var Hi;var qi;var Ui;var Yi;var Xi;var Vi;var Bi;var Gi;var Ki;var Ji;var Qi;var Zi;var er;var nr;var tr;var ar;var ir;var rr;var or;var sr;var lr;var ze;var fr;var zt;var ur;var Ft;var cr;var dr;var mr;var gr;var O;var Fe;var Pt;var Nt;var R;var Pe;var pr;var qe;var Ue;var An;var vr;var hr;var Ot;var Dt;var Mt;var br;var Ne;var zn;var Oe;var Ye;var De;var yr;var Et;var xr;var Sr;var wr;var Se;var Ir;var B;var Fn;var Tt;var Y;var m;var G;var T;var P;var Fr;var Dr;var Nn;var Mr;var D;var z;var _t;var $t;var fe;var Tr;var Ie;var K;var jr;var Wt;var Lr;var Be;var Ht;var qt;var Ut;var Yt;var Xt;var Vt;var Gt;var En;var Yr;var Xr;var Br;var Tn;var q;var U;var Gr;var Kt;var Jr;var Qr;var Zr;var C;var eo;var ke;var ro;var Ln;var _e;var V;var so;var Jt;var Je;var oe;var se;var Qe;var ue;var xo;var ko;var Co;var Ao;var zo;var Fo;var ta;var qn;var Un;var Re;var Po;var To;var jo;var Lo;var Vn;var _o;var Bn;var $o;var Ce;var Wo;var Ho;var qo;var Uo;var aa;var ia;var ra;var oa;var sa=w(()=>{xn=function(){},He={},Jn={},Qn=null,Zn={mark:xn,measure:xn};try{typeof window<`u`&&(He=window),typeof document<`u`&&(Jn=document),typeof MutationObserver<`u`&&(Qn=MutationObserver),typeof performance<`u`&&(Zn=performance)}catch{}wa=He.navigator||{},Sn=wa.userAgent,wn=Sn===void 0?``:Sn,j=He,h=Jn,In=Qn,ne=Zn,j.document,M=!!h.documentElement&&!!h.head&&typeof h.addEventListener==`function`&&typeof h.createElement==`function`,et=~wn.indexOf(`MSIE`)||~wn.indexOf(`Trident/`),Ia=/fa(k|kd|s|r|l|t|d|dr|dl|dt|b|slr|slpr|wsb|tl|ns|nds|es|gt|jr|jfr|jdr|usb|ufsb|udsb|cr|ss|sr|sl|st|sds|sdr|sdl|sdt|sldr|slpdr|pr|ms|vs)?[\-\ ]/,ka=/Font ?Awesome ?([567 ]*)(Solid|Regular|Light|Thin|Duotone|Brands|Free|Pro|Sharp Duotone|Sharp|Kit|Notdog Duo|Notdog|Chisel|Etch|Graphite|Thumbprint|Jelly Fill|Jelly Duo|Jelly|Utility|Utility Fill|Utility Duo|Slab Press|Slab|Slab Duo|Slab Press Duo|Pixel|Mosaic|Vellum|Whiteboard)?.*/i,nt={classic:{fa:`solid`,fas:`solid`,"fa-solid":`solid`,far:`regular`,"fa-regular":`regular`,fal:`light`,"fa-light":`light`,fat:`thin`,"fa-thin":`thin`,fab:`brands`,"fa-brands":`brands`},duotone:{fa:`solid`,fad:`solid`,"fa-solid":`solid`,"fa-duotone":`solid`,fadr:`regular`,"fa-regular":`regular`,fadl:`light`,"fa-light":`light`,fadt:`thin`,"fa-thin":`thin`},sharp:{fa:`solid`,fass:`solid`,"fa-solid":`solid`,fasr:`regular`,"fa-regular":`regular`,fasl:`light`,"fa-light":`light`,fast:`thin`,"fa-thin":`thin`},"sharp-duotone":{fa:`solid`,fasds:`solid`,"fa-solid":`solid`,fasdr:`regular`,"fa-regular":`regular`,fasdl:`light`,"fa-light":`light`,fasdt:`thin`,"fa-thin":`thin`},slab:{"fa-regular":`regular`,faslr:`regular`},"slab-press":{"fa-regular":`regular`,faslpr:`regular`},"slab-duo":{"fa-regular":`regular`,fasldr:`regular`},"slab-press-duo":{"fa-regular":`regular`,faslpdr:`regular`},thumbprint:{"fa-light":`light`,fatl:`light`},vellum:{"fa-solid":`solid`,favs:`solid`},pixel:{"fa-regular":`regular`,fapr:`regular`},mosaic:{"fa-solid":`solid`,fams:`solid`},whiteboard:{"fa-semibold":`semibold`,fawsb:`semibold`},notdog:{"fa-solid":`solid`,fans:`solid`},"notdog-duo":{"fa-solid":`solid`,fands:`solid`},etch:{"fa-solid":`solid`,faes:`solid`},graphite:{"fa-thin":`thin`,fagt:`thin`},jelly:{"fa-regular":`regular`,fajr:`regular`},"jelly-fill":{"fa-regular":`regular`,fajfr:`regular`},"jelly-duo":{"fa-regular":`regular`,fajdr:`regular`},chisel:{"fa-regular":`regular`,facr:`regular`},utility:{"fa-semibold":`semibold`,fausb:`semibold`},"utility-duo":{"fa-semibold":`semibold`,faudsb:`semibold`},"utility-fill":{"fa-semibold":`semibold`,faufsb:`semibold`}},Ca={GROUP:`duotone-group`,SWAP_OPACITY:`swap-opacity`,PRIMARY:`primary`,SECONDARY:`secondary`},tt=[`fa-classic`,`fa-duotone`,`fa-sharp`,`fa-sharp-duotone`,`fa-thumbprint`,`fa-whiteboard`,`fa-notdog`,`fa-notdog-duo`,`fa-chisel`,`fa-etch`,`fa-graphite`,`fa-jelly`,`fa-jelly-fill`,`fa-jelly-duo`,`fa-slab`,`fa-slab-press`,`fa-slab-press-duo`,`fa-slab-duo`,`fa-mosaic`,`fa-pixel`,`fa-vellum`,`fa-utility`,`fa-utility-duo`,`fa-utility-fill`],S=`classic`,J=`duotone`,at=`sharp`,it=`sharp-duotone`,rt=`chisel`,ot=`etch`,st=`graphite`,lt=`jelly`,ft=`jelly-duo`,ut=`jelly-fill`,ct=`mosaic`,dt=`notdog`,mt=`notdog-duo`,gt=`pixel`,pt=`slab`,vt=`slab-duo`,ht=`slab-press`,bt=`slab-press-duo`,yt=`thumbprint`,xt=`utility`,St=`utility-duo`,wt=`utility-fill`,It=`vellum`,kt=`whiteboard`,Aa=`Classic`,za=`Duotone`,Fa=`Sharp`,Pa=`Sharp Duotone`,Na=`Chisel`,Oa=`Etch`,Da=`Graphite`,Ma=`Jelly`,Ea=`Jelly Duo`,Ta=`Jelly Fill`,ja=`Mosaic`,La=`Notdog`,_a=`Notdog Duo`,$a=`Pixel`,Ra=`Slab`,Wa=`Slab Duo`,Ha=`Slab Press`,qa=`Slab Press Duo`,Ua=`Thumbprint`,Ya=`Utility`,Xa=`Utility Duo`,Va=`Utility Fill`,Ba=`Vellum`,Ga=`Whiteboard`,Ct=[S,J,at,it,rt,ot,st,lt,ft,ut,ct,dt,mt,gt,pt,vt,ht,bt,yt,xt,St,wt,It,kt],te={},g(g(g(g(g(g(g(g(g(g(te,S,Aa),J,za),at,Fa),it,Pa),rt,Na),ot,Oa),st,Da),lt,Ma),ft,Ea),ut,Ta),g(g(g(g(g(g(g(g(g(g(te,ct,ja),dt,La),mt,_a),gt,$a),pt,Ra),vt,Wa),ht,Ha),bt,qa),yt,Ua),xt,Ya),g(g(g(g(te,St,Xa),wt,Va),It,Ba),kt,Ga),Ka={classic:{900:`fas`,400:`far`,normal:`far`,300:`fal`,100:`fat`},duotone:{900:`fad`,400:`fadr`,300:`fadl`,100:`fadt`},sharp:{900:`fass`,400:`fasr`,300:`fasl`,100:`fast`},"sharp-duotone":{900:`fasds`,400:`fasdr`,300:`fasdl`,100:`fasdt`},slab:{400:`faslr`},"slab-press":{400:`faslpr`},"slab-duo":{400:`fasldr`},"slab-press-duo":{400:`faslpdr`},vellum:{900:`favs`},mosaic:{900:`fams`},pixel:{400:`fapr`},whiteboard:{600:`fawsb`},thumbprint:{300:`fatl`},notdog:{900:`fans`},"notdog-duo":{900:`fands`},etch:{900:`faes`},graphite:{100:`fagt`},chisel:{400:`facr`},jelly:{400:`fajr`},"jelly-fill":{400:`fajfr`},"jelly-duo":{400:`fajdr`},utility:{600:`fausb`},"utility-duo":{600:`faudsb`},"utility-fill":{600:`faufsb`}},Ja={"Font Awesome 7 Free":{900:`fas`,400:`far`},"Font Awesome 7 Pro":{900:`fas`,400:`far`,normal:`far`,300:`fal`,100:`fat`},"Font Awesome 7 Brands":{400:`fab`,normal:`fab`},"Font Awesome 7 Duotone":{900:`fad`,400:`fadr`,normal:`fadr`,300:`fadl`,100:`fadt`},"Font Awesome 7 Sharp":{900:`fass`,400:`fasr`,normal:`fasr`,300:`fasl`,100:`fast`},"Font Awesome 7 Sharp Duotone":{900:`fasds`,400:`fasdr`,normal:`fasdr`,300:`fasdl`,100:`fasdt`},"Font Awesome 7 Jelly":{400:`fajr`,normal:`fajr`},"Font Awesome 7 Jelly Fill":{400:`fajfr`,normal:`fajfr`},"Font Awesome 7 Jelly Duo":{400:`fajdr`,normal:`fajdr`},"Font Awesome 7 Slab":{400:`faslr`,normal:`faslr`},"Font Awesome 7 Slab Press":{400:`faslpr`,normal:`faslpr`},"Font Awesome 7 Slab Duo":{400:`fasldr`,normal:`fasldr`},"Font Awesome 7 Slab Press Duo":{400:`faslpdr`,normal:`faslpdr`},"Font Awesome 7 Pixel":{400:`fapr`,normal:`fapr`},"Font Awesome 7 Mosaic":{900:`fams`,normal:`fams`},"Font Awesome 7 Vellum":{900:`favs`,normal:`favs`},"Font Awesome 7 Thumbprint":{300:`fatl`,normal:`fatl`},"Font Awesome 7 Notdog":{900:`fans`,normal:`fans`},"Font Awesome 7 Notdog Duo":{900:`fands`,normal:`fands`},"Font Awesome 7 Etch":{900:`faes`,normal:`faes`},"Font Awesome 7 Graphite":{100:`fagt`,normal:`fagt`},"Font Awesome 7 Chisel":{400:`facr`,normal:`facr`},"Font Awesome 7 Whiteboard":{600:`fawsb`,normal:`fawsb`},"Font Awesome 7 Utility":{600:`fausb`,normal:`fausb`},"Font Awesome 7 Utility Duo":{600:`faudsb`,normal:`faudsb`},"Font Awesome 7 Utility Fill":{600:`faufsb`,normal:`faufsb`}},Qa=/* @__PURE__ */ new Map([[`classic`,{defaultShortPrefixId:`fas`,defaultStyleId:`solid`,styleIds:[`solid`,`regular`,`light`,`thin`,`brands`],futureStyleIds:[],defaultFontWeight:900}],[`duotone`,{defaultShortPrefixId:`fad`,defaultStyleId:`solid`,styleIds:[`solid`,`regular`,`light`,`thin`],futureStyleIds:[],defaultFontWeight:900}],[`sharp`,{defaultShortPrefixId:`fass`,defaultStyleId:`solid`,styleIds:[`solid`,`regular`,`light`,`thin`],futureStyleIds:[],defaultFontWeight:900}],[`sharp-duotone`,{defaultShortPrefixId:`fasds`,defaultStyleId:`solid`,styleIds:[`solid`,`regular`,`light`,`thin`],futureStyleIds:[],defaultFontWeight:900}],[`chisel`,{defaultShortPrefixId:`facr`,defaultStyleId:`regular`,styleIds:[`regular`],futureStyleIds:[],defaultFontWeight:400}],[`etch`,{defaultShortPrefixId:`faes`,defaultStyleId:`solid`,styleIds:[`solid`],futureStyleIds:[],defaultFontWeight:900}],[`graphite`,{defaultShortPrefixId:`fagt`,defaultStyleId:`thin`,styleIds:[`thin`],futureStyleIds:[],defaultFontWeight:100}],[`jelly`,{defaultShortPrefixId:`fajr`,defaultStyleId:`regular`,styleIds:[`regular`],futureStyleIds:[],defaultFontWeight:400}],[`jelly-duo`,{defaultShortPrefixId:`fajdr`,defaultStyleId:`regular`,styleIds:[`regular`],futureStyleIds:[],defaultFontWeight:400}],[`jelly-fill`,{defaultShortPrefixId:`fajfr`,defaultStyleId:`regular`,styleIds:[`regular`],futureStyleIds:[],defaultFontWeight:400}],[`mosaic`,{defaultShortPrefixId:`fams`,defaultStyleId:`solid`,styleIds:[`solid`],futureStyleIds:[],defaultFontWeight:900}],[`notdog`,{defaultShortPrefixId:`fans`,defaultStyleId:`solid`,styleIds:[`solid`],futureStyleIds:[],defaultFontWeight:900}],[`notdog-duo`,{defaultShortPrefixId:`fands`,defaultStyleId:`solid`,styleIds:[`solid`],futureStyleIds:[],defaultFontWeight:900}],[`pixel`,{defaultShortPrefixId:`fapr`,defaultStyleId:`regular`,styleIds:[`regular`],futureStyleIds:[],defaultFontWeight:400}],[`slab`,{defaultShortPrefixId:`faslr`,defaultStyleId:`regular`,styleIds:[`regular`],futureStyleIds:[],defaultFontWeight:400}],[`slab-duo`,{defaultShortPrefixId:`fasldr`,defaultStyleId:`regular`,styleIds:[`regular`],futureStyleIds:[],defaultFontWeight:400}],[`slab-press`,{defaultShortPrefixId:`faslpr`,defaultStyleId:`regular`,styleIds:[`regular`],futureStyleIds:[],defaultFontWeight:400}],[`slab-press-duo`,{defaultShortPrefixId:`faslpdr`,defaultStyleId:`regular`,styleIds:[`regular`],futureStyleIds:[],defaultFontWeight:400}],[`thumbprint`,{defaultShortPrefixId:`fatl`,defaultStyleId:`light`,styleIds:[`light`],futureStyleIds:[],defaultFontWeight:300}],[`utility`,{defaultShortPrefixId:`fausb`,defaultStyleId:`semibold`,styleIds:[`semibold`],futureStyleIds:[],defaultFontWeight:600}],[`utility-duo`,{defaultShortPrefixId:`faudsb`,defaultStyleId:`semibold`,styleIds:[`semibold`],futureStyleIds:[],defaultFontWeight:600}],[`utility-fill`,{defaultShortPrefixId:`faufsb`,defaultStyleId:`semibold`,styleIds:[`semibold`],futureStyleIds:[],defaultFontWeight:600}],[`vellum`,{defaultShortPrefixId:`favs`,defaultStyleId:`solid`,styleIds:[`solid`],futureStyleIds:[],defaultFontWeight:900}],[`whiteboard`,{defaultShortPrefixId:`fawsb`,defaultStyleId:`semibold`,styleIds:[`semibold`],futureStyleIds:[],defaultFontWeight:600}]]),Za={chisel:{regular:`facr`},classic:{brands:`fab`,light:`fal`,regular:`far`,solid:`fas`,thin:`fat`},duotone:{light:`fadl`,regular:`fadr`,solid:`fad`,thin:`fadt`},etch:{solid:`faes`},graphite:{thin:`fagt`},jelly:{regular:`fajr`},"jelly-duo":{regular:`fajdr`},"jelly-fill":{regular:`fajfr`},mosaic:{solid:`fams`},notdog:{solid:`fans`},"notdog-duo":{solid:`fands`},pixel:{regular:`fapr`},sharp:{light:`fasl`,regular:`fasr`,solid:`fass`,thin:`fast`},"sharp-duotone":{light:`fasdl`,regular:`fasdr`,solid:`fasds`,thin:`fasdt`},slab:{regular:`faslr`},"slab-duo":{regular:`fasldr`},"slab-press":{regular:`faslpr`},"slab-press-duo":{regular:`faslpdr`},thumbprint:{light:`fatl`},utility:{semibold:`fausb`},"utility-duo":{semibold:`faudsb`},"utility-fill":{semibold:`faufsb`},vellum:{solid:`favs`},whiteboard:{semibold:`fawsb`}},At=[`fak`,`fa-kit`,`fakd`,`fa-kit-duotone`],kn={kit:{fak:`kit`,"fa-kit":`kit`},"kit-duotone":{fakd:`kit-duotone`,"fa-kit-duotone":`kit-duotone`}},ei=[`kit`],ni=`kit`,ti=`kit-duotone`,ai=`Kit`,ii=`Kit Duotone`,g(g({},ni,ai),ti,ii),ri={kit:{"fa-kit":`fak`},"kit-duotone":{"fa-kit-duotone":`fakd`}},oi={"Font Awesome Kit":{400:`fak`,normal:`fak`},"Font Awesome Kit Duotone":{400:`fakd`,normal:`fakd`}},si={kit:{fak:`fa-kit`},"kit-duotone":{fakd:`fa-kit-duotone`}},Cn={kit:{kit:`fak`},"kit-duotone":{"kit-duotone":`fakd`}},ie={GROUP:`duotone-group`,SWAP_OPACITY:`swap-opacity`,PRIMARY:`primary`,SECONDARY:`secondary`},li=[`fa-classic`,`fa-duotone`,`fa-sharp`,`fa-sharp-duotone`,`fa-thumbprint`,`fa-whiteboard`,`fa-notdog`,`fa-notdog-duo`,`fa-chisel`,`fa-etch`,`fa-graphite`,`fa-jelly`,`fa-jelly-fill`,`fa-jelly-duo`,`fa-slab`,`fa-slab-press`,`fa-slab-press-duo`,`fa-slab-duo`,`fa-mosaic`,`fa-pixel`,`fa-vellum`,`fa-utility`,`fa-utility-duo`,`fa-utility-fill`],fi=`classic`,ui=`duotone`,ci=`sharp`,di=`sharp-duotone`,mi=`chisel`,gi=`etch`,pi=`graphite`,vi=`jelly`,hi=`jelly-duo`,bi=`jelly-fill`,yi=`mosaic`,xi=`notdog`,Si=`notdog-duo`,wi=`pixel`,Ii=`slab`,ki=`slab-duo`,Ci=`slab-press`,Ai=`slab-press-duo`,zi=`thumbprint`,Fi=`utility`,Pi=`utility-duo`,Ni=`utility-fill`,Oi=`vellum`,Di=`whiteboard`,Mi=`Classic`,Ei=`Duotone`,Ti=`Sharp`,ji=`Sharp Duotone`,Li=`Chisel`,_i=`Etch`,$i=`Graphite`,Ri=`Jelly`,Wi=`Jelly Duo`,Hi=`Jelly Fill`,qi=`Mosaic`,Ui=`Notdog`,Yi=`Notdog Duo`,Xi=`Pixel`,Vi=`Slab`,Bi=`Slab Duo`,Gi=`Slab Press`,Ki=`Slab Press Duo`,Ji=`Thumbprint`,Qi=`Utility`,Zi=`Utility Duo`,er=`Utility Fill`,nr=`Vellum`,tr=`Whiteboard`,ae={},g(g(g(g(g(g(g(g(g(g(ae,fi,Mi),ui,Ei),ci,Ti),di,ji),mi,Li),gi,_i),pi,$i),vi,Ri),hi,Wi),bi,Hi),g(g(g(g(g(g(g(g(g(g(ae,yi,qi),xi,Ui),Si,Yi),wi,Xi),Ii,Vi),ki,Bi),Ci,Gi),Ai,Ki),zi,Ji),Fi,Qi),g(g(g(g(ae,Pi,Zi),Ni,er),Oi,nr),Di,tr),ar=`kit`,ir=`kit-duotone`,rr=`Kit`,or=`Kit Duotone`,g(g({},ar,rr),ir,or),sr={classic:{"fa-brands":`fab`,"fa-duotone":`fad`,"fa-light":`fal`,"fa-regular":`far`,"fa-solid":`fas`,"fa-thin":`fat`},duotone:{"fa-regular":`fadr`,"fa-light":`fadl`,"fa-thin":`fadt`},sharp:{"fa-solid":`fass`,"fa-regular":`fasr`,"fa-light":`fasl`,"fa-thin":`fast`},"sharp-duotone":{"fa-solid":`fasds`,"fa-regular":`fasdr`,"fa-light":`fasdl`,"fa-thin":`fasdt`},slab:{"fa-regular":`faslr`},"slab-press":{"fa-regular":`faslpr`},"slab-duo":{"fa-regular":`fasldr`},"slab-press-duo":{"fa-regular":`faslpdr`},pixel:{"fa-regular":`fapr`},mosaic:{"fa-solid":`fams`},vellum:{"fa-solid":`favs`},whiteboard:{"fa-semibold":`fawsb`},thumbprint:{"fa-light":`fatl`},notdog:{"fa-solid":`fans`},"notdog-duo":{"fa-solid":`fands`},etch:{"fa-solid":`faes`},graphite:{"fa-thin":`fagt`},jelly:{"fa-regular":`fajr`},"jelly-fill":{"fa-regular":`fajfr`},"jelly-duo":{"fa-regular":`fajdr`},chisel:{"fa-regular":`facr`},utility:{"fa-semibold":`fausb`},"utility-duo":{"fa-semibold":`faudsb`},"utility-fill":{"fa-semibold":`faufsb`}},lr={classic:[`fas`,`far`,`fal`,`fat`,`fad`],duotone:[`fadr`,`fadl`,`fadt`],sharp:[`fass`,`fasr`,`fasl`,`fast`],"sharp-duotone":[`fasds`,`fasdr`,`fasdl`,`fasdt`],slab:[`faslr`],"slab-press":[`faslpr`],"slab-duo":[`fasldr`],"slab-press-duo":[`faslpdr`],pixel:[`fapr`],mosaic:[`fams`],vellum:[`favs`],whiteboard:[`fawsb`],thumbprint:[`fatl`],notdog:[`fans`],"notdog-duo":[`fands`],etch:[`faes`],graphite:[`fagt`],jelly:[`fajr`],"jelly-fill":[`fajfr`],"jelly-duo":[`fajdr`],chisel:[`facr`],utility:[`fausb`],"utility-duo":[`faudsb`],"utility-fill":[`faufsb`]},ze={classic:{fab:`fa-brands`,fad:`fa-duotone`,fal:`fa-light`,far:`fa-regular`,fas:`fa-solid`,fat:`fa-thin`},duotone:{fadr:`fa-regular`,fadl:`fa-light`,fadt:`fa-thin`},sharp:{fass:`fa-solid`,fasr:`fa-regular`,fasl:`fa-light`,fast:`fa-thin`},"sharp-duotone":{fasds:`fa-solid`,fasdr:`fa-regular`,fasdl:`fa-light`,fasdt:`fa-thin`},slab:{faslr:`fa-regular`},"slab-press":{faslpr:`fa-regular`},"slab-duo":{fasldr:`fa-regular`},"slab-press-duo":{faslpdr:`fa-regular`},pixel:{fapr:`fa-regular`},mosaic:{fams:`fa-solid`},vellum:{favs:`fa-solid`},whiteboard:{fawsb:`fa-semibold`},thumbprint:{fatl:`fa-light`},notdog:{fans:`fa-solid`},"notdog-duo":{fands:`fa-solid`},etch:{faes:`fa-solid`},graphite:{fagt:`fa-thin`},jelly:{fajr:`fa-regular`},"jelly-fill":{fajfr:`fa-regular`},"jelly-duo":{fajdr:`fa-regular`},chisel:{facr:`fa-regular`},utility:{fausb:`fa-semibold`},"utility-duo":{faudsb:`fa-semibold`},"utility-fill":{faufsb:`fa-semibold`}},fr=[`fa-solid`,`fa-regular`,`fa-light`,`fa-thin`,`fa-duotone`,`fa-brands`,`fa-semibold`],zt=[`fa`,`fas`,`far`,`fal`,`fat`,`fad`,`fadr`,`fadl`,`fadt`,`fab`,`fass`,`fasr`,`fasl`,`fast`,`fasds`,`fasdr`,`fasdl`,`fasdt`,`faslr`,`faslpr`,`fasldr`,`faslpdr`,`fapr`,`fams`,`favs`,`fawsb`,`fatl`,`fans`,`fands`,`faes`,`fagt`,`fajr`,`fajfr`,`fajdr`,`facr`,`fausb`,`faudsb`,`faufsb`].concat(li,fr),ur=[`solid`,`regular`,`light`,`thin`,`duotone`,`brands`,`semibold`],Ft=[1,2,3,4,5,6,7,8,9,10],cr=Ft.concat([11,12,13,14,15,16,17,18,19,20]),dr=[`aw`,`fw`,`pull-left`,`pull-right`],mr=[].concat(F(Object.keys(lr)),ur,dr,[`2xs`,`xs`,`sm`,`lg`,`xl`,`2xl`,`beat`,`beat-fade`,`border`,`bounce`,`buzz`,`canvas-square`,`canvas-roomy`,`fade`,`flip-360`,`flip-both`,`flip-horizontal`,`flip-vertical`,`flip`,`float`,`inverse`,`jello`,`layers`,`layers-bottom-left`,`layers-bottom-right`,`layers-counter`,`layers-text`,`layers-top-left`,`layers-top-right`,`li`,`pull-end`,`pull-start`,`pulse`,`rotate-180`,`rotate-270`,`rotate-90`,`rotate-by`,`shake`,`spin-pulse`,`spin-reverse`,`spin`,`spin-snap`,`spin-snap-4`,`spin-snap-8`,`stack-1x`,`stack-2x`,`stack`,`swing`,`ul`,`wag`,`width-auto`,`width-fixed`,ie.GROUP,ie.SWAP_OPACITY,ie.PRIMARY,ie.SECONDARY]).concat(Ft.map(function(e){return``.concat(e,`x`)})).concat(cr.map(function(e){return`w-`.concat(e)})),gr={"Font Awesome 5 Free":{900:`fas`,400:`far`},"Font Awesome 5 Pro":{900:`fas`,400:`far`,normal:`far`,300:`fal`},"Font Awesome 5 Brands":{400:`fab`,normal:`fab`},"Font Awesome 5 Duotone":{900:`fad`}},O=`___FONT_AWESOME___`,Fe=16,Pt=`fa`,Nt=`svg-inline--fa`,R=`data-fa-i2svg`,Pe=`data-fa-pseudo-element`,pr=`data-fa-pseudo-element-pending`,qe=`data-prefix`,Ue=`data-icon`,An=`fontawesome-i2svg`,vr=`async`,hr=[`HTML`,`HEAD`,`STYLE`,`SCRIPT`],Ot=[`::before`,`::after`,`:before`,`:after`],Dt=(function(){try{return!0}catch{return!1}})();Mt=f({},nt);Mt[S]=f(f(f(f({},{"fa-duotone":`duotone`}),nt[S]),kn.kit),kn[`kit-duotone`]);br=Q(Mt),Ne=f({},Za);Ne[S]=f(f(f(f({},{duotone:`fad`}),Ne[S]),Cn.kit),Cn[`kit-duotone`]);zn=Q(Ne),Oe=f({},ze);Oe[S]=f(f({},Oe[S]),si.kit);Ye=Q(Oe),De=f({},sr);De[S]=f(f({},De[S]),ri.kit);Q(De),yr=Ia,Et=`fa-layers-text`,xr=ka,Sr=f({},Ka),Q(Sr),wr=[`class`,`data-prefix`,`data-icon`,`data-fa-transform`,`data-fa-mask`],Se=Ca,Ir=[].concat(F(ei),F(mr)),B=j.FontAwesomeConfig||{};h&&typeof h.querySelector==`function`&&(Fn=[[`data-family-prefix`,`familyPrefix`],[`data-css-prefix`,`cssPrefix`],[`data-family-default`,`familyDefault`],[`data-style-default`,`styleDefault`],[`data-replacement-class`,`replacementClass`],[`data-auto-replace-svg`,`autoReplaceSvg`],[`data-auto-add-css`,`autoAddCss`],[`data-search-pseudo-elements`,`searchPseudoElements`],[`data-search-pseudo-elements-warnings`,`searchPseudoElementsWarnings`],[`data-search-pseudo-elements-full-scan`,`searchPseudoElementsFullScan`],[`data-observe-mutations`,`observeMutations`],[`data-mutate-approach`,`mutateApproach`],[`data-keep-original-source`,`keepOriginalSource`],[`data-measure-performance`,`measurePerformance`],[`data-show-missing-icons`,`showMissingIcons`]],Fn.forEach(function(e){var t=ce(e,2),n=t[0],a=t[1],i=Cr(kr(n));i!=null&&(B[a]=i)}));Tt={styleDefault:`solid`,familyDefault:S,cssPrefix:Pt,replacementClass:Nt,autoReplaceSvg:!0,autoAddCss:!0,searchPseudoElements:!1,searchPseudoElementsWarnings:!0,searchPseudoElementsFullScan:!1,observeMutations:!0,mutateApproach:`async`,keepOriginalSource:!0,measurePerformance:!1,showMissingIcons:!0};B.familyPrefix&&(B.cssPrefix=B.familyPrefix);Y=f(f({},Tt),B);Y.autoReplaceSvg||(Y.observeMutations=!1);m={};Object.keys(Tt).forEach(function(e){Object.defineProperty(m,e,{enumerable:!0,set:function(n){Y[e]=n,G.forEach(function(a){return a(m)})},get:function(){return Y[e]}})});Object.defineProperty(m,"familyPrefix",{enumerable:!0,set:function(t){Y.cssPrefix=t,G.forEach(function(n){return n(m)})},get:function(){return Y.cssPrefix}});j.FontAwesomeConfig=m;G=[];T=Fe,P={size:16,x:0,y:0,rotate:0,flipX:!1,flipY:!1};Fr=`0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ`;Dr=`:root, :host {
  --fa-font-solid: normal 900 1em/1 'Font Awesome 7 Free';
  --fa-font-regular: normal 400 1em/1 'Font Awesome 7 Free';
  --fa-font-light: normal 300 1em/1 'Font Awesome 7 Pro';
  --fa-font-thin: normal 100 1em/1 'Font Awesome 7 Pro';
  --fa-font-duotone: normal 900 1em/1 'Font Awesome 7 Duotone';
  --fa-font-duotone-regular: normal 400 1em/1 'Font Awesome 7 Duotone';
  --fa-font-duotone-light: normal 300 1em/1 'Font Awesome 7 Duotone';
  --fa-font-duotone-thin: normal 100 1em/1 'Font Awesome 7 Duotone';
  --fa-font-brands: normal 400 1em/1 'Font Awesome 7 Brands';
  --fa-font-sharp-solid: normal 900 1em/1 'Font Awesome 7 Sharp';
  --fa-font-sharp-regular: normal 400 1em/1 'Font Awesome 7 Sharp';
  --fa-font-sharp-light: normal 300 1em/1 'Font Awesome 7 Sharp';
  --fa-font-sharp-thin: normal 100 1em/1 'Font Awesome 7 Sharp';
  --fa-font-sharp-duotone-solid: normal 900 1em/1 'Font Awesome 7 Sharp Duotone';
  --fa-font-sharp-duotone-regular: normal 400 1em/1 'Font Awesome 7 Sharp Duotone';
  --fa-font-sharp-duotone-light: normal 300 1em/1 'Font Awesome 7 Sharp Duotone';
  --fa-font-sharp-duotone-thin: normal 100 1em/1 'Font Awesome 7 Sharp Duotone';
  --fa-font-slab-regular: normal 400 1em/1 'Font Awesome 7 Slab';
  --fa-font-slab-press-regular: normal 400 1em/1 'Font Awesome 7 Slab Press';
  --fa-font-slab-duo-regular: normal 400 1em/1 'Font Awesome 7 Slab Duo';
  --fa-font-slab-press-duo-regular: normal 400 1em/1 'Font Awesome 7 Slab Press Duo';
  --fa-font-pixel-regular: normal 400 1em/1 'Font Awesome 7 Pixel';
  --fa-font-mosaic-solid: normal 900 1em/1 'Font Awesome 7 Mosaic';
  --fa-font-vellum-solid: normal 900 1em/1 'Font Awesome 7 Vellum';
  --fa-font-whiteboard-semibold: normal 600 1em/1 'Font Awesome 7 Whiteboard';
  --fa-font-thumbprint-light: normal 300 1em/1 'Font Awesome 7 Thumbprint';
  --fa-font-notdog-solid: normal 900 1em/1 'Font Awesome 7 Notdog';
  --fa-font-notdog-duo-solid: normal 900 1em/1 'Font Awesome 7 Notdog Duo';
  --fa-font-etch-solid: normal 900 1em/1 'Font Awesome 7 Etch';
  --fa-font-graphite-thin: normal 100 1em/1 'Font Awesome 7 Graphite';
  --fa-font-jelly-regular: normal 400 1em/1 'Font Awesome 7 Jelly';
  --fa-font-jelly-fill-regular: normal 400 1em/1 'Font Awesome 7 Jelly Fill';
  --fa-font-jelly-duo-regular: normal 400 1em/1 'Font Awesome 7 Jelly Duo';
  --fa-font-chisel-regular: normal 400 1em/1 'Font Awesome 7 Chisel';
  --fa-font-utility-semibold: normal 600 1em/1 'Font Awesome 7 Utility';
  --fa-font-utility-duo-semibold: normal 600 1em/1 'Font Awesome 7 Utility Duo';
  --fa-font-utility-fill-semibold: normal 600 1em/1 'Font Awesome 7 Utility Fill';
}

.svg-inline--fa {
  box-sizing: content-box;
  display: var(--fa-display, inline-block);
  height: 1em;
  overflow: visible;
  vertical-align: -0.125em;
  width: var(--fa-width, 1.25em);
}
.svg-inline--fa.fa-2xs {
  vertical-align: 0.1em;
}
.svg-inline--fa.fa-xs {
  vertical-align: 0em;
}
.svg-inline--fa.fa-sm {
  vertical-align: -0.0714285714em;
}
.svg-inline--fa.fa-lg {
  vertical-align: -0.2em;
}
.svg-inline--fa.fa-xl {
  vertical-align: -0.25em;
}
.svg-inline--fa.fa-2xl {
  vertical-align: -0.3125em;
}
.svg-inline--fa.fa-pull-left,
.svg-inline--fa .fa-pull-start {
  float: inline-start;
  margin-inline-end: var(--fa-pull-margin, 0.3em);
}
.svg-inline--fa.fa-pull-right,
.svg-inline--fa .fa-pull-end {
  float: inline-end;
  margin-inline-start: var(--fa-pull-margin, 0.3em);
}
.svg-inline--fa.fa-li {
  width: var(--fa-li-width, 2em);
  inset-inline-start: calc(-1 * var(--fa-li-width, 2em));
  inset-block-start: 0.25em; /* syncing vertical alignment with Web Font rendering */
}

.fa-layers-counter, .fa-layers-text {
  display: inline-block;
  position: absolute;
  text-align: center;
}

.fa-layers {
  display: inline-block;
  height: 1em;
  position: relative;
  text-align: center;
  vertical-align: -0.125em;
  width: var(--fa-width, 1.25em);
}
.fa-layers .svg-inline--fa {
  inset: 0;
  margin: auto;
  position: absolute;
  transform-origin: center center;
}

.fa-layers-text {
  left: 50%;
  top: 50%;
  transform: translate(-50%, -50%);
  transform-origin: center center;
}

.fa-layers-counter {
  background-color: var(--fa-counter-background-color, #ff253a);
  border-radius: var(--fa-counter-border-radius, 1em);
  box-sizing: border-box;
  color: var(--fa-inverse, #fff);
  line-height: var(--fa-counter-line-height, 1);
  max-width: var(--fa-counter-max-width, 5em);
  min-width: var(--fa-counter-min-width, 1.5em);
  overflow: hidden;
  padding: var(--fa-counter-padding, 0.25em 0.5em);
  right: var(--fa-right, 0);
  text-overflow: ellipsis;
  top: var(--fa-top, 0);
  transform: scale(var(--fa-counter-scale, 0.25));
  transform-origin: top right;
}

.fa-layers-bottom-right {
  bottom: var(--fa-bottom, 0);
  right: var(--fa-right, 0);
  top: auto;
  transform: scale(var(--fa-layers-scale, 0.25));
  transform-origin: bottom right;
}

.fa-layers-bottom-left {
  bottom: var(--fa-bottom, 0);
  left: var(--fa-left, 0);
  right: auto;
  top: auto;
  transform: scale(var(--fa-layers-scale, 0.25));
  transform-origin: bottom left;
}

.fa-layers-top-right {
  top: var(--fa-top, 0);
  right: var(--fa-right, 0);
  transform: scale(var(--fa-layers-scale, 0.25));
  transform-origin: top right;
}

.fa-layers-top-left {
  left: var(--fa-left, 0);
  right: auto;
  top: var(--fa-top, 0);
  transform: scale(var(--fa-layers-scale, 0.25));
  transform-origin: top left;
}

.fa-1x {
  font-size: 1em;
}

.fa-2x {
  font-size: 2em;
}

.fa-3x {
  font-size: 3em;
}

.fa-4x {
  font-size: 4em;
}

.fa-5x {
  font-size: 5em;
}

.fa-6x {
  font-size: 6em;
}

.fa-7x {
  font-size: 7em;
}

.fa-8x {
  font-size: 8em;
}

.fa-9x {
  font-size: 9em;
}

.fa-10x {
  font-size: 10em;
}

.fa-2xs {
  font-size: calc(10 / 16 * 1em); /* converts a 10px size into an em-based value that's relative to the scale's 16px base */
  line-height: calc(1 / 10 * 1em); /* sets the line-height of the icon back to that of it's parent */
  vertical-align: calc((6 / 10 - 0.375) * 1em); /* vertically centers the icon taking into account the surrounding text's descender */
}

.fa-xs {
  font-size: calc(12 / 16 * 1em); /* converts a 12px size into an em-based value that's relative to the scale's 16px base */
  line-height: calc(1 / 12 * 1em); /* sets the line-height of the icon back to that of it's parent */
  vertical-align: calc((6 / 12 - 0.375) * 1em); /* vertically centers the icon taking into account the surrounding text's descender */
}

.fa-sm {
  font-size: calc(14 / 16 * 1em); /* converts a 14px size into an em-based value that's relative to the scale's 16px base */
  line-height: calc(1 / 14 * 1em); /* sets the line-height of the icon back to that of it's parent */
  vertical-align: calc((6 / 14 - 0.375) * 1em); /* vertically centers the icon taking into account the surrounding text's descender */
}

.fa-lg {
  font-size: calc(20 / 16 * 1em); /* converts a 20px size into an em-based value that's relative to the scale's 16px base */
  line-height: calc(1 / 20 * 1em); /* sets the line-height of the icon back to that of it's parent */
  vertical-align: calc((6 / 20 - 0.375) * 1em); /* vertically centers the icon taking into account the surrounding text's descender */
}

.fa-xl {
  font-size: calc(24 / 16 * 1em); /* converts a 24px size into an em-based value that's relative to the scale's 16px base */
  line-height: calc(1 / 24 * 1em); /* sets the line-height of the icon back to that of it's parent */
  vertical-align: calc((6 / 24 - 0.375) * 1em); /* vertically centers the icon taking into account the surrounding text's descender */
}

.fa-2xl {
  font-size: calc(32 / 16 * 1em); /* converts a 32px size into an em-based value that's relative to the scale's 16px base */
  line-height: calc(1 / 32 * 1em); /* sets the line-height of the icon back to that of it's parent */
  vertical-align: calc((6 / 32 - 0.375) * 1em); /* vertically centers the icon taking into account the surrounding text's descender */
}

.fa-width-auto {
  --fa-width: auto;
}

.fa-fw,
.fa-width-fixed {
  --fa-width: 1.25em;
}

.fa-canvas-square {
  padding-block: 0.125em;
  margin-block-end: -0.125em;
}

.fa-canvas-roomy {
  padding-block: 0.25em;
  padding-inline: 0.125em;
  margin-block-end: -0.25em;
  box-sizing: content-box;
}

.fa-ul {
  list-style-type: none;
  margin-inline-start: var(--fa-li-margin, 2.5em);
  padding-inline-start: 0;
}
.fa-ul > li {
  position: relative;
}

.fa-li {
  inset-inline-start: calc(-1 * var(--fa-li-width, 2em));
  position: absolute;
  text-align: center;
  width: var(--fa-li-width, 2em);
  line-height: inherit;
}

/* Heads Up: Bordered Icons will not be supported in the future!
  - This feature will be deprecated in the next major release of Font Awesome (v8)!
  - You may continue to use it in this version *v7), but it will not be supported in Font Awesome v8.
*/
/* Notes:
* --@{v.$css-prefix}-border-width = 1/16 by default (to render as ~1px based on a 16px default font-size)
* --@{v.$css-prefix}-border-padding =
  ** 3/16 for vertical padding (to give ~2px of vertical whitespace around an icon considering it's vertical alignment)
  ** 4/16 for horizontal padding (to give ~4px of horizontal whitespace around an icon)
*/
.fa-border {
  border-color: var(--fa-border-color, #eee);
  border-radius: var(--fa-border-radius, 0.1em);
  border-style: var(--fa-border-style, solid);
  border-width: var(--fa-border-width, 0.0625em);
  box-sizing: var(--fa-border-box-sizing, content-box);
  padding: var(--fa-border-padding, 0.1875em 0.25em);
}

.fa-pull-left,
.fa-pull-start {
  float: inline-start;
  margin-inline-end: var(--fa-pull-margin, 0.3em);
}

.fa-pull-right,
.fa-pull-end {
  float: inline-end;
  margin-inline-start: var(--fa-pull-margin, 0.3em);
}

.fa-beat {
  animation-name: fa-beat;
  animation-delay: var(--fa-animation-delay, 0s);
  animation-direction: var(--fa-animation-direction, normal);
  animation-duration: var(--fa-animation-duration, 1s);
  animation-iteration-count: var(--fa-animation-iteration-count, infinite);
  animation-timing-function: var(--fa-animation-timing, ease-in-out);
}

.fa-bounce {
  animation-name: fa-bounce;
  animation-delay: var(--fa-animation-delay, 0s);
  animation-direction: var(--fa-animation-direction, normal);
  animation-duration: var(--fa-animation-duration, 1s);
  animation-iteration-count: var(--fa-animation-iteration-count, infinite);
  animation-timing-function: var(--fa-animation-timing, cubic-bezier(0.28, 0.84, 0.42, 1));
}

.fa-fade {
  animation-name: fa-fade;
  animation-delay: var(--fa-animation-delay, 0s);
  animation-direction: var(--fa-animation-direction, normal);
  animation-duration: var(--fa-animation-duration, 1s);
  animation-iteration-count: var(--fa-animation-iteration-count, infinite);
  animation-timing-function: var(--fa-animation-timing, ease-in-out);
}

.fa-beat-fade {
  animation-name: fa-beat-fade;
  animation-delay: var(--fa-animation-delay, 0s);
  animation-direction: var(--fa-animation-direction, normal);
  animation-duration: var(--fa-animation-duration, 1s);
  animation-iteration-count: var(--fa-animation-iteration-count, infinite);
  animation-timing-function: var(--fa-animation-timing, ease-in-out);
}

.fa-flip {
  animation-name: fa-flip;
  animation-delay: var(--fa-animation-delay, 0s);
  animation-direction: var(--fa-animation-direction, normal);
  animation-duration: var(--fa-animation-duration, 1.5s);
  animation-iteration-count: var(--fa-animation-iteration-count, infinite);
  animation-timing-function: var(--fa-animation-timing, ease-in-out);
}

.fa-flip-360 {
  animation-name: fa-flip-360;
  animation-delay: var(--fa-animation-delay, 0s);
  animation-direction: var(--fa-animation-direction, normal);
  animation-duration: var(--fa-animation-duration, 1s);
  animation-iteration-count: var(--fa-animation-iteration-count, infinite);
  animation-timing-function: var(--fa-animation-timing, ease-in-out);
}

.fa-shake {
  animation-name: fa-shake;
  animation-delay: var(--fa-animation-delay, 0s);
  animation-direction: var(--fa-animation-direction, normal);
  animation-duration: var(--fa-animation-duration, 0.75s);
  animation-iteration-count: var(--fa-animation-iteration-count, infinite);
  animation-timing-function: var(--fa-animation-timing, ease-in-out);
}

.fa-spin {
  animation-name: fa-spin;
  animation-delay: var(--fa-animation-delay, 0s);
  animation-direction: var(--fa-animation-direction, normal);
  animation-duration: var(--fa-animation-duration, 2s);
  animation-iteration-count: var(--fa-animation-iteration-count, infinite);
  animation-timing-function: var(--fa-animation-timing, linear);
}

.fa-spin-reverse {
  --fa-animation-direction: reverse;
}

.fa-pulse,
.fa-spin-pulse {
  animation-name: fa-spin;
  animation-direction: var(--fa-animation-direction, normal);
  animation-duration: var(--fa-animation-duration, 1s);
  animation-iteration-count: var(--fa-animation-iteration-count, infinite);
  animation-timing-function: var(--fa-animation-timing, steps(8));
}

.fa-spin-snap {
  animation-name: fa-spin-snap;
  animation-delay: var(--fa-animation-delay, 0s);
  animation-direction: var(--fa-animation-direction, normal);
  animation-duration: var(--fa-animation-duration, 3s);
  animation-iteration-count: var(--fa-animation-iteration-count, infinite);
  animation-timing-function: var(--fa-animation-timing, linear);
}

.fa-spin-snap-4 {
  animation-name: fa-spin-snap-4;
  animation-delay: var(--fa-animation-delay, 0s);
  animation-direction: var(--fa-animation-direction, normal);
  animation-duration: var(--fa-animation-duration, 2.4s);
  animation-iteration-count: var(--fa-animation-iteration-count, infinite);
  animation-timing-function: var(--fa-animation-timing, linear);
}

.fa-spin-snap-8 {
  animation-name: fa-spin-snap-8;
  animation-delay: var(--fa-animation-delay, 0s);
  animation-direction: var(--fa-animation-direction, normal);
  animation-duration: var(--fa-animation-duration, 4s);
  animation-iteration-count: var(--fa-animation-iteration-count, infinite);
  animation-timing-function: var(--fa-animation-timing, linear);
}

.fa-buzz {
  animation-name: fa-buzz;
  animation-delay: var(--fa-animation-delay, 0s);
  animation-direction: var(--fa-animation-direction, normal);
  animation-duration: var(--fa-animation-duration, 0.6s);
  animation-iteration-count: var(--fa-animation-iteration-count, infinite);
  animation-timing-function: var(--fa-animation-timing, linear);
}

.fa-wag {
  animation-name: fa-wag;
  animation-delay: var(--fa-animation-delay, 0s);
  animation-direction: var(--fa-animation-direction, normal);
  animation-duration: var(--fa-animation-duration, 0.9s);
  animation-iteration-count: var(--fa-animation-iteration-count, infinite);
  animation-timing-function: var(--fa-animation-timing, ease-out);
  transform-origin: bottom center;
}

.fa-float {
  animation-name: fa-float;
  animation-delay: var(--fa-animation-delay, 0s);
  animation-direction: var(--fa-animation-direction, normal);
  animation-duration: var(--fa-animation-duration, 3s);
  animation-iteration-count: var(--fa-animation-iteration-count, infinite);
  animation-timing-function: var(--fa-animation-timing, ease-in-out);
  will-change: transform;
}

.fa-swing {
  animation-name: fa-swing;
  animation-delay: var(--fa-animation-delay, 0s);
  animation-direction: var(--fa-animation-direction, normal);
  animation-duration: var(--fa-animation-duration, 1.2s);
  animation-iteration-count: var(--fa-animation-iteration-count, infinite);
  animation-timing-function: var(--fa-animation-timing, ease-out);
  transform-origin: top center;
}

.fa-jello {
  animation-name: fa-jello;
  animation-delay: var(--fa-animation-delay, 0s);
  animation-direction: var(--fa-animation-direction, normal);
  animation-duration: var(--fa-animation-duration, 0.9s);
  animation-iteration-count: var(--fa-animation-iteration-count, infinite);
  animation-timing-function: var(--fa-animation-timing, ease-out);
}

@media (prefers-reduced-motion: reduce) {
  .fa-beat,
  .fa-bounce,
  .fa-fade,
  .fa-beat-fade,
  .fa-flip,
  .fa-flip-360,
  .fa-pulse,
  .fa-shake,
  .fa-spin,
  .fa-spin-pulse,
  .fa-buzz,
  .fa-float,
  .fa-jello,
  .fa-spin-snap,
  .fa-spin-snap-4,
  .fa-spin-snap-8,
  .fa-swing,
  .fa-wag {
    animation: none !important;
    transition: none !important;
  }
}
@keyframes fa-beat {
  0% {
    transform: scale(1);
  }
  25% {
    transform: scale(calc(1.25 * var(--fa-beat-scale, 1.25)));
  }
  45% {
    transform: scale(calc(1.22 * var(--fa-beat-scale, 1.22)));
  }
  65% {
    transform: scale(calc(1.25 * var(--fa-beat-scale, 1.25)));
  }
  90% {
    transform: scale(1);
  }
}
@keyframes fa-bounce {
  0% {
    transform: scale(1, 1) translateY(0);
    animation-timing-function: var(--fa-animation-timing);
  }
  14% {
    transform: scale(var(--fa-bounce-start-scale-x, 1.06), var(--fa-bounce-start-scale-y, 0.94)) translateY(var(--fa-bounce-anticipation, 3px));
    animation-timing-function: cubic-bezier(0.33, 0, 0.66, 0.33);
  }
  32% {
    transform: scale(var(--fa-bounce-jump-scale-x, 0.94), var(--fa-bounce-jump-scale-y, 1.12)) translateY(calc(-1 * var(--fa-bounce-height, 0.5em)));
    animation-timing-function: cubic-bezier(0.33, 0.66, 0.66, 1);
  }
  52% {
    transform: scale(1, 1) translateY(calc(-1 * var(--fa-bounce-height, 0.5em) * 1.1));
    animation-timing-function: cubic-bezier(0.5, 0, 1, 0.5);
  }
  70% {
    transform: scale(var(--fa-bounce-land-scale-x, 1.06), var(--fa-bounce-land-scale-y, 0.92)) translateY(0);
    animation-timing-function: cubic-bezier(0.33, 0.33, 0.66, 1);
  }
  85% {
    transform: scale(0.98, 1.04) translateY(calc(-2px * var(--fa-bounce-rebound, 1)));
    animation-timing-function: cubic-bezier(0.33, 0, 0.66, 1);
  }
  100% {
    transform: scale(1, 1) translateY(0);
  }
}
@keyframes fa-fade {
  0% {
    opacity: 1;
    transform: scale(1);
    animation-timing-function: cubic-bezier(0.2, 0, 0.4, 1);
  }
  40% {
    opacity: var(--fa-fade-opacity, 0.4);
    transform: scale(0.98);
    animation-timing-function: cubic-bezier(0.4, 0, 0.6, 1);
  }
  100% {
    opacity: 1;
    transform: scale(1);
  }
}
@keyframes fa-beat-fade {
  0% {
    opacity: var(--fa-beat-fade-opacity, 0.4);
    transform: scale(1);
    animation-timing-function: cubic-bezier(0.2, 0, 0.4, 1);
  }
  25% {
    opacity: calc(var(--fa-beat-fade-opacity, 0.4) + 0.4);
    transform: scale(var(--fa-beat-fade-scale, 1.28));
    animation-timing-function: cubic-bezier(0.4, 0, 0.6, 1);
  }
  45% {
    opacity: 1;
    transform: scale(var(--fa-beat-fade-scale, 1.25));
    animation-timing-function: cubic-bezier(0.4, 0, 0.2, 1);
  }
  65% {
    opacity: calc(var(--fa-beat-fade-opacity, 0.4) + 0.4);
    transform: scale(var(--fa-beat-fade-scale, 1.28));
    animation-timing-function: cubic-bezier(0.4, 0, 0.6, 1);
  }
  100% {
    opacity: var(--fa-beat-fade-opacity, 0.4);
    transform: scale(1);
  }
}
@keyframes fa-flip {
  0% {
    transform: perspective(2em) scale(1) rotate3d(var(--fa-flip-x, 0), var(--fa-flip-y, 1), var(--fa-flip-z, 0), 0deg);
    animation-timing-function: cubic-bezier(0.2, 0, 0.4, 1);
  }
  8% {
    transform: perspective(2em) scale(var(--fa-flip-anticipation-scale, 0.95)) rotate3d(var(--fa-flip-x, 0), var(--fa-flip-y, 1), var(--fa-flip-z, 0), 0deg);
    animation-timing-function: cubic-bezier(0.33, 0, 0.66, 0.33);
  }
  35% {
    transform: perspective(2em) scale(1) rotate3d(var(--fa-flip-x, 0), var(--fa-flip-y, 1), var(--fa-flip-z, 0), calc(var(--fa-flip-angle, -360deg) * 0.6));
    animation-timing-function: linear;
  }
  65% {
    transform: perspective(2em) scale(1) rotate3d(var(--fa-flip-x, 0), var(--fa-flip-y, 1), var(--fa-flip-z, 0), calc(var(--fa-flip-angle, -360deg) * 0.5));
    animation-timing-function: cubic-bezier(0.33, 0.66, 0.66, 1);
  }
  92% {
    transform: perspective(2em) scale(1) rotate3d(var(--fa-flip-x, 0), var(--fa-flip-y, 1), var(--fa-flip-z, 0), calc(var(--fa-flip-angle, -360deg) * var(--fa-flip-overshoot, 1.04)));
    animation-timing-function: cubic-bezier(0.33, 0, 0.66, 1);
  }
  100% {
    transform: perspective(2em) scale(1) rotate3d(var(--fa-flip-x, 0), var(--fa-flip-y, 1), var(--fa-flip-z, 0), var(--fa-flip-angle, -360deg));
  }
}
@keyframes fa-flip-360 {
  0% {
    transform: perspective(2em) scale(1) rotate3d(var(--fa-flip-x, 0), var(--fa-flip-y, 1), var(--fa-flip-z, 0), 0deg);
    animation-timing-function: cubic-bezier(0.2, 0, 0.4, 1);
  }
  8% {
    transform: perspective(2em) scale(var(--fa-flip-anticipation-scale, 0.95)) rotate3d(var(--fa-flip-x, 0), var(--fa-flip-y, 1), var(--fa-flip-z, 0), 0deg);
    animation-timing-function: cubic-bezier(0.33, 0, 0.66, 0.33);
  }
  50% {
    transform: perspective(2em) scale(1) rotate3d(var(--fa-flip-x, 0), var(--fa-flip-y, 1), var(--fa-flip-z, 0), calc(var(--fa-flip-angle, -360deg) * 0.6));
    animation-timing-function: cubic-bezier(0.33, 0.66, 0.66, 1);
  }
  80% {
    transform: perspective(2em) scale(1) rotate3d(var(--fa-flip-x, 0), var(--fa-flip-y, 1), var(--fa-flip-z, 0), calc(var(--fa-flip-angle, -360deg) * var(--fa-flip-overshoot, 1.04)));
    animation-timing-function: cubic-bezier(0.33, 0, 0.66, 1);
  }
  100% {
    transform: perspective(2em) scale(1) rotate3d(var(--fa-flip-x, 0), var(--fa-flip-y, 1), var(--fa-flip-z, 0), var(--fa-flip-angle, -360deg));
  }
}
@keyframes fa-shake {
  0% {
    transform: rotate(0deg);
    animation-timing-function: cubic-bezier(0.2, 0, 0.8, 1);
  }
  8% {
    transform: rotate(35deg) translateX(1px);
    animation-timing-function: cubic-bezier(0.3, 0, 0.7, 1);
  }
  20% {
    transform: rotate(-22deg) translateX(-1px);
    animation-timing-function: cubic-bezier(0.3, 0, 0.7, 1);
  }
  35% {
    transform: rotate(15deg) translateX(1px);
    animation-timing-function: cubic-bezier(0.3, 0, 0.7, 1);
  }
  50% {
    transform: rotate(-9deg);
    animation-timing-function: cubic-bezier(0.4, 0, 0.6, 1);
  }
  65% {
    transform: rotate(5deg);
    animation-timing-function: cubic-bezier(0.4, 0, 0.6, 1);
  }
  78% {
    transform: rotate(-3deg);
    animation-timing-function: cubic-bezier(0.4, 0, 0.6, 1);
  }
  90% {
    transform: rotate(1deg);
    animation-timing-function: cubic-bezier(0.4, 0, 0.2, 1);
  }
  100% {
    transform: rotate(0deg);
  }
}
@keyframes fa-spin {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}
@keyframes fa-spin-snap {
  0% {
    transform: rotate(0deg);
    animation-timing-function: cubic-bezier(0, 0, 0.2, 1);
  }
  12% {
    transform: rotate(60deg);
    animation-timing-function: cubic-bezier(0.8, 0, 1, 1);
  }
  16.67% {
    transform: rotate(60deg);
    animation-timing-function: cubic-bezier(0, 0, 0.2, 1);
  }
  28.67% {
    transform: rotate(120deg);
    animation-timing-function: cubic-bezier(0.8, 0, 1, 1);
  }
  33.33% {
    transform: rotate(120deg);
    animation-timing-function: cubic-bezier(0, 0, 0.2, 1);
  }
  45.33% {
    transform: rotate(180deg);
    animation-timing-function: cubic-bezier(0.8, 0, 1, 1);
  }
  50% {
    transform: rotate(180deg);
    animation-timing-function: cubic-bezier(0, 0, 0.2, 1);
  }
  62% {
    transform: rotate(240deg);
    animation-timing-function: cubic-bezier(0.8, 0, 1, 1);
  }
  66.67% {
    transform: rotate(240deg);
    animation-timing-function: cubic-bezier(0, 0, 0.2, 1);
  }
  78.67% {
    transform: rotate(300deg);
    animation-timing-function: cubic-bezier(0.8, 0, 1, 1);
  }
  83.33% {
    transform: rotate(300deg);
    animation-timing-function: cubic-bezier(0, 0, 0.2, 1);
  }
  95.33% {
    transform: rotate(360deg);
    animation-timing-function: cubic-bezier(0.8, 0, 1, 1);
  }
  100% {
    transform: rotate(360deg);
  }
}
@keyframes fa-spin-snap-4 {
  0% {
    transform: rotate(0deg);
    animation-timing-function: cubic-bezier(0, 0, 0.2, 1);
  }
  15% {
    transform: rotate(90deg);
    animation-timing-function: cubic-bezier(0.8, 0, 1, 1);
  }
  25% {
    transform: rotate(90deg);
    animation-timing-function: cubic-bezier(0, 0, 0.2, 1);
  }
  40% {
    transform: rotate(180deg);
    animation-timing-function: cubic-bezier(0.8, 0, 1, 1);
  }
  50% {
    transform: rotate(180deg);
    animation-timing-function: cubic-bezier(0, 0, 0.2, 1);
  }
  65% {
    transform: rotate(270deg);
    animation-timing-function: cubic-bezier(0.8, 0, 1, 1);
  }
  75% {
    transform: rotate(270deg);
    animation-timing-function: cubic-bezier(0, 0, 0.2, 1);
  }
  90% {
    transform: rotate(360deg);
    animation-timing-function: cubic-bezier(0.8, 0, 1, 1);
  }
  100% {
    transform: rotate(360deg);
  }
}
@keyframes fa-spin-snap-8 {
  0% {
    transform: rotate(0deg);
    animation-timing-function: cubic-bezier(0, 0, 0.2, 1);
  }
  9% {
    transform: rotate(45deg);
    animation-timing-function: cubic-bezier(0.8, 0, 1, 1);
  }
  12.5% {
    transform: rotate(45deg);
    animation-timing-function: cubic-bezier(0, 0, 0.2, 1);
  }
  21.5% {
    transform: rotate(90deg);
    animation-timing-function: cubic-bezier(0.8, 0, 1, 1);
  }
  25% {
    transform: rotate(90deg);
    animation-timing-function: cubic-bezier(0, 0, 0.2, 1);
  }
  34% {
    transform: rotate(135deg);
    animation-timing-function: cubic-bezier(0.8, 0, 1, 1);
  }
  37.5% {
    transform: rotate(135deg);
    animation-timing-function: cubic-bezier(0, 0, 0.2, 1);
  }
  46.5% {
    transform: rotate(180deg);
    animation-timing-function: cubic-bezier(0.8, 0, 1, 1);
  }
  50% {
    transform: rotate(180deg);
    animation-timing-function: cubic-bezier(0, 0, 0.2, 1);
  }
  59% {
    transform: rotate(225deg);
    animation-timing-function: cubic-bezier(0.8, 0, 1, 1);
  }
  62.5% {
    transform: rotate(225deg);
    animation-timing-function: cubic-bezier(0, 0, 0.2, 1);
  }
  71.5% {
    transform: rotate(270deg);
    animation-timing-function: cubic-bezier(0.8, 0, 1, 1);
  }
  75% {
    transform: rotate(270deg);
    animation-timing-function: cubic-bezier(0, 0, 0.2, 1);
  }
  84% {
    transform: rotate(315deg);
    animation-timing-function: cubic-bezier(0.8, 0, 1, 1);
  }
  87.5% {
    transform: rotate(315deg);
    animation-timing-function: cubic-bezier(0, 0, 0.2, 1);
  }
  96.5% {
    transform: rotate(360deg);
    animation-timing-function: cubic-bezier(0.8, 0, 1, 1);
  }
  100% {
    transform: rotate(360deg);
  }
}
@keyframes fa-buzz {
  0% {
    transform: translateX(0) rotate(0deg);
    animation-timing-function: cubic-bezier(0.1, 0, 0.9, 1);
  }
  5% {
    transform: translateX(var(--fa-buzz-distance, 4px)) rotate(0.5deg);
  }
  10% {
    transform: translateX(calc(-1 * var(--fa-buzz-distance, 4px))) rotate(-0.5deg);
  }
  15% {
    transform: translateX(var(--fa-buzz-distance, 4px)) rotate(0.3deg);
  }
  20% {
    transform: translateX(calc(-1 * var(--fa-buzz-distance, 4px))) rotate(-0.3deg);
  }
  25% {
    transform: translateX(calc(var(--fa-buzz-distance, 4px) * 0.7)) rotate(0.2deg);
  }
  30% {
    transform: translateX(calc(-1 * var(--fa-buzz-distance, 4px) * 0.7)) rotate(-0.2deg);
  }
  35% {
    transform: translateX(calc(var(--fa-buzz-distance, 4px) * 0.4)) rotate(0.1deg);
  }
  40% {
    transform: translateX(0) rotate(0deg);
  }
  100% {
    transform: translateX(0) rotate(0deg);
  }
}
@keyframes fa-wag {
  0% {
    transform: rotate(0deg);
    animation-timing-function: cubic-bezier(0.2, 0, 0.6, 1);
  }
  12% {
    transform: rotate(var(--fa-wag-angle, 12deg));
    animation-timing-function: cubic-bezier(0.4, 0, 0.2, 1);
  }
  24% {
    transform: rotate(2deg);
    animation-timing-function: cubic-bezier(0.2, 0, 0.6, 1);
  }
  36% {
    transform: rotate(calc(var(--fa-wag-angle, 12deg) * 0.85));
    animation-timing-function: cubic-bezier(0.4, 0, 0.2, 1);
  }
  48% {
    transform: rotate(1deg);
    animation-timing-function: cubic-bezier(0.2, 0, 0.6, 1);
  }
  58% {
    transform: rotate(calc(var(--fa-wag-angle, 12deg) * 0.6));
    animation-timing-function: cubic-bezier(0.4, 0, 0.2, 1);
  }
  68% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(0deg);
  }
}
@keyframes fa-float {
  0% {
    transform: translateY(0) translateX(0) rotate(0deg) scale(var(--fa-float-squash-x, 1.02), var(--fa-float-squash-y, 0.98));
    animation-timing-function: cubic-bezier(0.33, 0, 0.66, 0.33);
  }
  15% {
    transform: translateY(calc(-0.4 * var(--fa-float-height, 6px))) translateX(var(--fa-float-drift, 1px)) rotate(var(--fa-float-tilt, 1deg)) scale(1, 1);
    animation-timing-function: cubic-bezier(0.33, 0.66, 0.66, 1);
  }
  35% {
    transform: translateY(calc(-1 * var(--fa-float-height, 6px))) translateX(0) rotate(0deg) scale(var(--fa-float-stretch-x, 0.98), var(--fa-float-stretch-y, 1.03));
    animation-timing-function: cubic-bezier(0.5, 0, 0.5, 0);
  }
  50% {
    transform: translateY(calc(-0.92 * var(--fa-float-height, 6px))) translateX(calc(-0.5 * var(--fa-float-drift, 1px))) rotate(calc(-0.5 * var(--fa-float-tilt, 1deg))) scale(0.995, 1.01);
    animation-timing-function: cubic-bezier(0.33, 0, 0.66, 0.33);
  }
  70% {
    transform: translateY(calc(-0.3 * var(--fa-float-height, 6px))) translateX(calc(-1 * var(--fa-float-drift, 1px))) rotate(calc(-1 * var(--fa-float-tilt, 1deg))) scale(1, 1);
    animation-timing-function: cubic-bezier(0.33, 0.66, 0.66, 1);
  }
  90% {
    transform: translateY(calc(0.05 * var(--fa-float-height, 6px))) translateX(0) rotate(0deg) scale(var(--fa-float-squash-x, 1.02), var(--fa-float-squash-y, 0.98));
    animation-timing-function: cubic-bezier(0.33, 0, 0.66, 1);
  }
  100% {
    transform: translateY(0) translateX(0) rotate(0deg) scale(var(--fa-float-squash-x, 1.02), var(--fa-float-squash-y, 0.98));
  }
}
@keyframes fa-swing {
  0% {
    transform: rotate(0deg);
    animation-timing-function: cubic-bezier(0.2, 0, 0.8, 1);
  }
  8% {
    transform: rotate(var(--fa-swing-angle, 22deg));
    animation-timing-function: cubic-bezier(0.3, 0, 0.7, 1);
  }
  18% {
    transform: rotate(calc(-1 * var(--fa-swing-angle, 22deg) * 0.85));
    animation-timing-function: cubic-bezier(0.3, 0, 0.7, 1);
  }
  28% {
    transform: rotate(calc(var(--fa-swing-angle, 22deg) * 0.65));
    animation-timing-function: cubic-bezier(0.35, 0, 0.65, 1);
  }
  38% {
    transform: rotate(calc(-1 * var(--fa-swing-angle, 22deg) * 0.45));
    animation-timing-function: cubic-bezier(0.4, 0, 0.6, 1);
  }
  48% {
    transform: rotate(calc(var(--fa-swing-angle, 22deg) * 0.25));
    animation-timing-function: cubic-bezier(0.4, 0, 0.6, 1);
  }
  56% {
    transform: rotate(calc(-1 * var(--fa-swing-angle, 22deg) * 0.1));
    animation-timing-function: cubic-bezier(0.4, 0, 0.6, 1);
  }
  64% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(0deg);
  }
}
@keyframes fa-jello {
  0% {
    transform: scale(1, 1);
    animation-timing-function: cubic-bezier(0.2, 0, 0.8, 1);
  }
  12% {
    transform: scale(var(--fa-jello-scale-x, 1.15), calc(2 - var(--fa-jello-scale-x, 1.15)));
    animation-timing-function: cubic-bezier(0.3, 0, 0.7, 1);
  }
  24% {
    transform: scale(calc(2 - var(--fa-jello-scale-y, 1.12)), var(--fa-jello-scale-y, 1.12));
    animation-timing-function: cubic-bezier(0.3, 0, 0.7, 1);
  }
  36% {
    transform: scale(calc(1 + (var(--fa-jello-scale-x, 1.15) - 1) * 0.5), calc(2 - (1 + (var(--fa-jello-scale-x, 1.15) - 1) * 0.5)));
    animation-timing-function: cubic-bezier(0.4, 0, 0.6, 1);
  }
  48% {
    transform: scale(calc(2 - (1 + (var(--fa-jello-scale-y, 1.12) - 1) * 0.3)), calc(1 + (var(--fa-jello-scale-y, 1.12) - 1) * 0.3));
    animation-timing-function: cubic-bezier(0.4, 0, 0.6, 1);
  }
  58% {
    transform: scale(1.02, 0.98);
    animation-timing-function: cubic-bezier(0.4, 0, 0.2, 1);
  }
  68% {
    transform: scale(1, 1);
  }
  100% {
    transform: scale(1, 1);
  }
}
.fa-rotate-90 {
  transform: rotate(90deg);
}

.fa-rotate-180 {
  transform: rotate(180deg);
}

.fa-rotate-270 {
  transform: rotate(270deg);
}

.fa-flip-horizontal {
  transform: scale(-1, 1);
}

.fa-flip-vertical {
  transform: scale(1, -1);
}

.fa-flip-both,
.fa-flip-horizontal.fa-flip-vertical {
  transform: scale(-1, -1);
}

.fa-rotate-by {
  transform: rotate(var(--fa-rotate-angle, 0));
}

.svg-inline--fa .fa-primary {
  fill: var(--fa-primary-color, currentColor);
  opacity: var(--fa-primary-opacity, 1);
}

.svg-inline--fa .fa-secondary {
  fill: var(--fa-secondary-color, currentColor);
  opacity: var(--fa-secondary-opacity, 0.4);
}

.svg-inline--fa.fa-swap-opacity .fa-primary {
  opacity: var(--fa-secondary-opacity, 0.4);
}

.svg-inline--fa.fa-swap-opacity .fa-secondary {
  opacity: var(--fa-primary-opacity, 1);
}

.svg-inline--fa mask .fa-primary,
.svg-inline--fa mask .fa-secondary {
  fill: black;
}

.svg-inline--fa.fa-inverse {
  fill: var(--fa-inverse, #fff);
}

.fa-stack {
  display: inline-block;
  height: 2em;
  line-height: 2em;
  position: relative;
  vertical-align: middle;
  width: 2.5em;
}

.fa-inverse {
  color: var(--fa-inverse, #fff);
}

.svg-inline--fa.fa-stack-1x {
  --fa-width: 1.25em;
  height: 1em;
  width: var(--fa-width);
}
.svg-inline--fa.fa-stack-2x {
  --fa-width: 2.5em;
  height: 2em;
  width: var(--fa-width);
}

.fa-stack-1x,
.fa-stack-2x {
  inset: 0;
  margin: auto;
  position: absolute;
  z-index: var(--fa-stack-z-index, auto);
}`;Nn=!1;Mr={mixout:function(){return{dom:{css:Lt,insertCss:we}}},hooks:function(){return{beforeDOMElementCreation:function(){we()},beforeI2svg:function(){we()}}}},D=j||{};D[O]||(D[O]={});D[O].styles||(D[O].styles={});D[O].hooks||(D[O].hooks={});D[O].shims||(D[O].shims=[]);z=D[O],_t=[],$t=function(){h.removeEventListener(`DOMContentLoaded`,$t),fe=1,_t.map(function(t){return t()})},fe=!1;M&&(fe=(h.documentElement.doScroll?/^loaded|^c/:/^loaded|^i|^c/).test(h.readyState),fe||h.addEventListener(`DOMContentLoaded`,$t));Tr=function(t,n){return function(a,i,r,o){return t.call(n,a,i,r,o)}},Ie=function(t,n,a,i){var r=Object.keys(t),o=r.length,s=i!==void 0?Tr(n,i):n,l,u,d;for(a===void 0?(l=1,d=t[r[0]]):(l=0,d=a);l<o;l++)u=r[l],d=s(d,t[u],u,t);return d};K=z.styles,jr=z.shims,Wt=Object.keys(Ye),Lr=Wt.reduce(function(e,t){return e[t]=Object.keys(Ye[t]),e},{}),Be=null,Ht={},qt={},Ut={},Yt={},Xt={};Vt=function(){var t=function(r){return Ie(K,function(o,s,l){return o[l]=Ie(s,r,{}),o},{})};Ht=t(function(i,r,o){if(r[3]&&(i[r[3]]=o),r[2])r[2].filter(function(l){return typeof l==`number`}).forEach(function(l){i[l.toString(16)]=o});return i}),qt=t(function(i,r,o){if(i[o]=o,r[2])r[2].filter(function(l){return typeof l==`string`}).forEach(function(l){i[l]=o});return i}),Xt=t(function(i,r,o){var s=r[2];return i[o]=o,s.forEach(function(l){i[l]=o}),i});var n=`far`in K||m.autoFetchSvg,a=Ie(jr,function(i,r){var o=r[0],s=r[1],l=r[2];return s===`far`&&!n&&(s=`fas`),typeof o==`string`&&(i.names[o]={prefix:s,iconName:l}),typeof o==`number`&&(i.unicodes[o.toString(16)]={prefix:s,iconName:l}),i},{names:{},unicodes:{}});Ut=a.names,Yt=a.unicodes,Be=me(m.styleDefault,{family:m.familyDefault})};Ar(function(e){Be=me(e.styleDefault,{family:m.familyDefault})});Vt();Gt=function(){return{prefix:null,iconName:null,rest:[]}};En=zt.concat(At);Yr=Ct.filter(function(e){return e!==S||e!==J}),Xr=Object.keys(ze).filter(function(e){return e!==S}).map(function(e){return Object.keys(ze[e])}).flat();Br=(function(){function e(){pa(this,e),this.definitions={}}return va(e,[{key:`add`,value:function(){for(var n=this,a=arguments.length,i=new Array(a),r=0;r<a;r++)i[r]=arguments[r];var o=i.reduce(this._pullDefinitions,{});Object.keys(o).forEach(function(s){n.definitions[s]=f(f({},n.definitions[s]||{}),o[s]),Me(s,o[s]);var l=Ye[S][s];l&&Me(l,o[s]),Vt()})}},{key:`reset`,value:function(){this.definitions={}}},{key:`_pullDefinitions`,value:function(n,a){var i=a.prefix&&a.iconName&&a.icon?{0:a}:a;return Object.keys(i).map(function(r){var o=i[r],s=o.prefix,l=o.iconName,u=o.icon,d=u[2];n[s]||(n[s]={}),d.length>0&&d.forEach(function(c){typeof c==`string`&&(n[s][c]=u)}),n[s][l]=u}),n}}])})(),Tn=[],q={},U={},Gr=Object.keys(U);Kt=new Br,Jr=function(){m.autoReplaceSvg=!1,m.observeMutations=!1,W(`noAuto`)},Qr={i2svg:function(){var t=arguments.length>0&&arguments[0]!==void 0?arguments[0]:{};return M?(W(`beforeI2svg`,t),_(`pseudoElements2svg`,t),_(`i2svg`,t)):Promise.reject(/* @__PURE__ */ new Error(`Operation requires a DOM of some kind.`))},watch:function(){var t=arguments.length>0&&arguments[0]!==void 0?arguments[0]:{},n=t.autoReplaceSvgRoot;m.autoReplaceSvg===!1&&(m.autoReplaceSvg=!0),m.observeMutations=!0,Er(function(){eo({autoReplaceSvgRoot:n}),W(`watch`,t)})}},Zr={icon:function(t){if(t===null)return null;if(le(t)===`object`&&t.prefix&&t.iconName)return{prefix:t.prefix,iconName:$(t.prefix,t.iconName)||t.iconName};if(Array.isArray(t)&&t.length===2){var n=t[1].indexOf(`fa-`)===0?t[1].slice(3):t[1],a=me(t[0]);return{prefix:a,iconName:$(a,n)||n}}if(typeof t==`string`&&(t.indexOf(``.concat(m.cssPrefix,`-`))>-1||t.match(yr))){var i=ge(t.split(` `),{skipLookups:!0});return{prefix:i.prefix||L(),iconName:$(i.prefix,i.iconName)||i.iconName}}if(typeof t==`string`){var r=L();return{prefix:r,iconName:$(r,t)||t}}}},C={noAuto:Jr,config:m,dom:Qr,parse:Zr,library:Kt,findIconDefinition:Te,toHtml:Z},eo=function(){var n=(arguments.length>0&&arguments[0]!==void 0?arguments[0]:{}).autoReplaceSvgRoot,a=n===void 0?h:n;(Object.keys(z.styles).length>0||m.autoFetchSvg)&&M&&m.autoReplaceSvg&&C.dom.i2svg({node:a})};ke=z.styles;ro={found:!1,width:512,height:512};Ln=function(){},_e=m.measurePerformance&&ne&&ne.mark&&ne.measure?ne:{mark:Ln,measure:Ln},V=`FA "7.3.1"`,so=function(t){return _e.mark(``.concat(V,` `).concat(t,` begins`)),function(){return Jt(t)}},Jt=function(t){_e.mark(``.concat(V,` `).concat(t,` ends`)),_e.measure(``.concat(V,` `).concat(t),``.concat(V,` `).concat(t,` begins`),``.concat(V,` `).concat(t,` ends`))},Je={begin:so,end:Jt},oe=function(){};se={replace:function(t){var n=t[0];if(n.parentNode)if(t[1].forEach(function(i){n.parentNode.insertBefore(Qt(i),n)}),n.getAttribute(R)===null&&m.keepOriginalSource){var a=h.createComment(go(n));n.parentNode.replaceChild(a,n)}else n.remove()},nest:function(t){var n=t[0],a=t[1];if(~Xe(n).indexOf(m.replacementClass))return se.replace(t);var i=new RegExp(``.concat(m.cssPrefix,`-.*`));if(delete a[0].attributes.id,a[0].attributes.class){var r=a[0].attributes.class.split(` `).reduce(function(s,l){return l===m.replacementClass||l.match(i)?s.toSvg.push(l):s.toNode.push(l),s},{toNode:[],toSvg:[]});a[0].attributes.class=r.toSvg.join(` `),r.toNode.length===0?n.removeAttribute(`class`):n.setAttribute(`class`,r.toNode.join(` `))}var o=a.map(function(s){return Z(s)}).join(`
`);n.setAttribute(R,``),n.innerHTML=o}};Qe=!1;ue=null;xo=z.styles;ko=function(t){var n=arguments.length>1&&arguments[1]!==void 0?arguments[1]:{},a=n.transform,i=a===void 0?P:a,r=n.symbol,o=r===void 0?!1:r,s=n.mask,l=s===void 0?null:s,u=n.maskId,d=u===void 0?null:u,c=n.classes,v=c===void 0?[]:c,p=n.attributes,y=p===void 0?{}:p,b=n.styles,w=b===void 0?{}:b;if(t){var x=t.prefix,k=t.iconName,A=t.icon;return pe(f({type:`icon`},t),function(){return W(`beforeDOMElementCreation`,{iconDefinition:t,params:n}),Ke({icons:{main:je(A),mask:l?je(l.icon):{found:!1,width:null,height:null,icon:{}}},prefix:x,iconName:k,transform:f(f({},P),i),symbol:o,maskId:d,extra:{attributes:y,styles:w,classes:v}})})}},Co={mixout:function(){return{icon:Io(ko)}},hooks:function(){return{mutationObserverCallbacks:function(n){return n.treeCallback=Hn,n.nodeCallback=wo,n}}},provides:function(t){t.i2svg=function(n){var a=n.node,i=a===void 0?h:a,r=n.callback;return Hn(i,r===void 0?function(){}:r)},t.generateSvgReplacementMutation=function(n,a){var i=a.iconName,r=a.prefix,o=a.transform,s=a.symbol,l=a.mask,u=a.maskId,d=a.extra;return new Promise(function(c,v){Promise.all([Le(i,r),l.iconName?Le(l.iconName,l.prefix):Promise.resolve({found:!1,width:512,height:512,icon:{}})]).then(function(p){var y=ce(p,2),b=y[0],w=y[1];c([n,Ke({icons:{main:b,mask:w},prefix:r,iconName:i,transform:o,symbol:s,maskId:u,extra:d,watchable:!0})])}).catch(v)})},t.generateAbstractIcon=function(n){var a=n.children,i=n.attributes,r=n.main,o=n.transform,s=n.styles,l=de(s);l.length>0&&(i.style=l);var u;return Ve(o)&&(u=_(`generateAbstractTransformGrouping`,{main:r,transform:o,containerWidth:r.width,iconWidth:r.width})),a.push(u||r.icon),{children:a,attributes:i}}}},Ao={mixout:function(){return{layer:function(n){var a=arguments.length>1&&arguments[1]!==void 0?arguments[1]:{},i=a.classes,r=i===void 0?[]:i;return pe({type:`layer`},function(){W(`beforeDOMElementCreation`,{assembler:n,params:a});var o=[];return n(function(s){Array.isArray(s)?s.map(function(l){o=o.concat(l.abstract)}):o=o.concat(s.abstract)}),[{tag:`span`,attributes:{class:[``.concat(m.cssPrefix,`-layers`)].concat(F(r)).join(` `)},children:o}]})}}}},zo={mixout:function(){return{counter:function(n){var a=arguments.length>1&&arguments[1]!==void 0?arguments[1]:{},i=a.title,r=i===void 0?null:i,o=a.classes,s=o===void 0?[]:o,l=a.attributes,u=l===void 0?{}:l,d=a.styles,c=d===void 0?{}:d;return pe({type:`counter`,content:n},function(){return W(`beforeDOMElementCreation`,{content:n,params:a}),io({content:n.toString(),title:r,extra:{attributes:u,styles:c,classes:[``.concat(m.cssPrefix,`-layers-counter`)].concat(F(s))}})})}}}},Fo={mixout:function(){return{text:function(n){var a=arguments.length>1&&arguments[1]!==void 0?arguments[1]:{},i=a.transform,r=i===void 0?P:i,o=a.classes,s=o===void 0?[]:o,l=a.attributes,u=l===void 0?{}:l,d=a.styles,c=d===void 0?{}:d;return pe({type:`text`,content:n},function(){return W(`beforeDOMElementCreation`,{content:n,params:a}),jn({content:n,transform:f(f({},P),r),extra:{attributes:u,styles:c,classes:[``.concat(m.cssPrefix,`-layers-text`)].concat(F(s))}})})}}},provides:function(t){t.generateLayersText=function(n,a){var i=a.transform,r=a.extra,o=null,s=null;if(et){var l=parseInt(getComputedStyle(n).fontSize,10),u=n.getBoundingClientRect();o=u.width/l,s=u.height/l}return Promise.resolve([n,jn({content:n.innerHTML,width:o,height:s,transform:i,extra:r,watchable:!0})])}}},ta=new RegExp(`"`,`ug`),qn=[1105920,1112319],Un=f(f(f(f({},{FontAwesome:{normal:`fas`,400:`fas`}}),Ja),gr),oi),Re=Object.keys(Un).reduce(function(e,t){return e[t.toLowerCase()]=Un[t],e},{}),Po=Object.keys(Re).reduce(function(e,t){var n=Re[t];return e[t]=n[900]||F(Object.entries(n))[0][1],e},{});To=function(t){return!!t&&Ot.some(function(n){return t.includes(n)})},jo=function(t){if(!t)return[];var n=/* @__PURE__ */ new Set,a=t.split(/,(?![^()]*\))/).map(function(l){return l.trim()});a=a.flatMap(function(l){return l.includes(`(`)?l:l.split(`,`).map(function(u){return u.trim()})});var i=re(a),r;try{for(i.s();!(r=i.n()).done;){var o=r.value;if(To(o)){var s=Ot.reduce(function(l,u){return l.replace(u,``)},o);s!==``&&s!==`*`&&n.add(s)}}}catch(l){i.e(l)}finally{i.f()}return n};Lo={hooks:function(){return{mutationObserverCallbacks:function(n){return n.pseudoElementsCallback=Xn,n}}},provides:function(t){t.pseudoElements2svg=function(n){var a=n.node,i=a===void 0?h:a;m.searchPseudoElements&&Xn(i)}}},Vn=!1,_o={mixout:function(){return{dom:{unwatch:function(){ea(),Vn=!0}}}},hooks:function(){return{bootstrap:function(){Rn(Ee(`mutationObserverCallbacks`,{}))},noAuto:function(){po()},watch:function(n){var a=n.observeMutationsRoot;Vn?$e():Rn(Ee(`mutationObserverCallbacks`,{observeMutationsRoot:a}))}}}},Bn=function(t){return t.toLowerCase().split(` `).reduce(function(a,i){var r=i.toLowerCase().split(`-`),o=r[0],s=r.slice(1).join(`-`);if(o&&s===`h`)return a.flipX=!0,a;if(o&&s===`v`)return a.flipY=!0,a;if(s=parseFloat(s),isNaN(s))return a;switch(o){case`grow`:a.size=a.size+s;break;case`shrink`:a.size=a.size-s;break;case`left`:a.x=a.x-s;break;case`right`:a.x=a.x+s;break;case`up`:a.y=a.y-s;break;case`down`:a.y=a.y+s;break;case`rotate`:a.rotate=a.rotate+s}return a},{size:16,x:0,y:0,flipX:!1,flipY:!1,rotate:0})},$o={mixout:function(){return{parse:{transform:function(n){return Bn(n)}}}},hooks:function(){return{parseNodeAttributes:function(n,a){var i=a.getAttribute(`data-fa-transform`);return i&&(n.transform=Bn(i)),n}}},provides:function(t){t.generateAbstractTransformGrouping=function(n){var a=n.main,i=n.transform,r=n.containerWidth,o=n.iconWidth,s={transform:`translate(`.concat(r/2,` 256)`)},l=`translate(`.concat(i.x*32,`, `).concat(i.y*32,`) `),u=`scale(`.concat(i.size/16*(i.flipX?-1:1),`, `).concat(i.size/16*(i.flipY?-1:1),`) `),d=`rotate(`.concat(i.rotate,` 0 0)`),p={outer:s,inner:{transform:``.concat(l,` `).concat(u,` `).concat(d)},path:{transform:`translate(`.concat(o/2*-1,` -256)`)}};return{tag:`g`,attributes:f({},p.outer),children:[{tag:`g`,attributes:f({},p.inner),children:[{tag:a.icon.tag,children:a.icon.children,attributes:f(f({},a.icon.attributes),p.path)}]}]}}}},Ce={x:0,y:0,width:`100%`,height:`100%`};Wo={hooks:function(){return{parseNodeAttributes:function(n,a){var i=a.getAttribute(`data-fa-mask`),r=i?ge(i.split(` `).map(function(o){return o.trim()})):Gt();return r.prefix||(r.prefix=L()),n.mask=r,n.maskId=a.getAttribute(`data-fa-mask-id`),n}}},provides:function(t){t.generateAbstractMask=function(n){var a=n.children,i=n.attributes,r=n.main,o=n.mask,s=n.maskId,l=n.transform,u=r.width,d=r.icon,c=o.width,v=o.icon,p=Nr({transform:l,containerWidth:c,iconWidth:u}),y={tag:`rect`,attributes:f(f({},Ce),{},{fill:`white`})},b=d.children?{children:d.children.map(Gn)}:{},w={tag:`g`,attributes:f({},p.inner),children:[Gn(f({tag:d.tag,attributes:f(f({},d.attributes),p.path)},b))]},x={tag:`g`,attributes:f({},p.outer),children:[w]},k=`mask-`.concat(s||Pn()),A=`clip-`.concat(s||Pn()),H={tag:`mask`,attributes:f(f({},Ce),{},{id:k,maskUnits:`userSpaceOnUse`,maskContentUnits:`userSpaceOnUse`}),children:[y,x]},N={tag:`defs`,children:[{tag:`clipPath`,attributes:{id:A},children:Ro(v)},H]};return a.push(N,{tag:`rect`,attributes:f({fill:`currentColor`,"clip-path":`url(#`.concat(A,`)`),mask:`url(#`.concat(k,`)`)},Ce)}),{children:a,attributes:i}}}},Ho={provides:function(t){var n=!1;j.matchMedia&&(n=j.matchMedia(`(prefers-reduced-motion: reduce)`).matches),t.missingIconAbstract=function(){var a=[],i={fill:`currentColor`},r={attributeType:`XML`,repeatCount:`indefinite`,dur:`2s`};a.push({tag:`path`,attributes:f(f({},i),{},{d:`M156.5,447.7l-12.6,29.5c-18.7-9.5-35.9-21.2-51.5-34.9l22.7-22.7C127.6,430.5,141.5,440,156.5,447.7z M40.6,272H8.5 c1.4,21.2,5.4,41.7,11.7,61.1L50,321.2C45.1,305.5,41.8,289,40.6,272z M40.6,240c1.4-18.8,5.2-37,11.1-54.1l-29.5-12.6 C14.7,194.3,10,216.7,8.5,240H40.6z M64.3,156.5c7.8-14.9,17.2-28.8,28.1-41.5L69.7,92.3c-13.7,15.6-25.5,32.8-34.9,51.5 L64.3,156.5z M397,419.6c-13.9,12-29.4,22.3-46.1,30.4l11.9,29.8c20.7-9.9,39.8-22.6,56.9-37.6L397,419.6z M115,92.4 c13.9-12,29.4-22.3,46.1-30.4l-11.9-29.8c-20.7,9.9-39.8,22.6-56.8,37.6L115,92.4z M447.7,355.5c-7.8,14.9-17.2,28.8-28.1,41.5 l22.7,22.7c13.7-15.6,25.5-32.9,34.9-51.5L447.7,355.5z M471.4,272c-1.4,18.8-5.2,37-11.1,54.1l29.5,12.6 c7.5-21.1,12.2-43.5,13.6-66.8H471.4z M321.2,462c-15.7,5-32.2,8.2-49.2,9.4v32.1c21.2-1.4,41.7-5.4,61.1-11.7L321.2,462z M240,471.4c-18.8-1.4-37-5.2-54.1-11.1l-12.6,29.5c21.1,7.5,43.5,12.2,66.8,13.6V471.4z M462,190.8c5,15.7,8.2,32.2,9.4,49.2h32.1 c-1.4-21.2-5.4-41.7-11.7-61.1L462,190.8z M92.4,397c-12-13.9-22.3-29.4-30.4-46.1l-29.8,11.9c9.9,20.7,22.6,39.8,37.6,56.9 L92.4,397z M272,40.6c18.8,1.4,36.9,5.2,54.1,11.1l12.6-29.5C317.7,14.7,295.3,10,272,8.5V40.6z M190.8,50 c15.7-5,32.2-8.2,49.2-9.4V8.5c-21.2,1.4-41.7,5.4-61.1,11.7L190.8,50z M442.3,92.3L419.6,115c12,13.9,22.3,29.4,30.5,46.1 l29.8-11.9C470,128.5,457.3,109.4,442.3,92.3z M397,92.4l22.7-22.7c-15.6-13.7-32.8-25.5-51.5-34.9l-12.6,29.5 C370.4,72.1,384.4,81.5,397,92.4z`})});var o=f(f({},r),{},{attributeName:`opacity`}),s={tag:`circle`,attributes:f(f({},i),{},{cx:`256`,cy:`364`,r:`28`}),children:[]};return n||s.children.push({tag:`animate`,attributes:f(f({},r),{},{attributeName:`r`,values:`28;14;28;28;14;28;`})},{tag:`animate`,attributes:f(f({},o),{},{values:`1;0;1;1;0;1;`})}),a.push(s),a.push({tag:`path`,attributes:f(f({},i),{},{opacity:`1`,d:`M263.7,312h-16c-6.6,0-12-5.4-12-12c0-71,77.4-63.9,77.4-107.8c0-20-17.8-40.2-57.4-40.2c-29.1,0-44.3,9.6-59.2,28.7 c-3.9,5-11.1,6-16.2,2.4l-13.1-9.2c-5.6-3.9-6.9-11.8-2.6-17.2c21.2-27.2,46.4-44.7,91.2-44.7c52.3,0,97.4,29.8,97.4,80.2 c0,67.6-77.4,63.5-77.4,107.8C275.7,306.6,270.3,312,263.7,312z`}),children:n?[]:[{tag:`animate`,attributes:f(f({},o),{},{values:`1;0;0;0;0;1;`})}]}),n||a.push({tag:`path`,attributes:f(f({},i),{},{opacity:`0`,d:`M232.5,134.5l7,168c0.3,6.4,5.6,11.5,12,11.5h9c6.4,0,11.7-5.1,12-11.5l7-168c0.3-6.8-5.2-12.5-12-12.5h-23 C237.7,122,232.2,127.7,232.5,134.5z`}),children:[{tag:`animate`,attributes:f(f({},o),{},{values:`0;0;1;1;0;0;`})}]}),{tag:`g`,attributes:{class:`missing`},children:a}}}},qo={hooks:function(){return{parseNodeAttributes:function(n,a){var i=a.getAttribute(`data-fa-symbol`);return n.symbol=i===null?!1:i===``?!0:i,n}}}},Uo=[Mr,Co,Ao,zo,Fo,Lo,_o,$o,Wo,Ho,qo];Kr(Uo,{mixoutsTo:C});C.noAuto,aa=C.config,C.library,ia=C.dom,ra=C.parse,C.findIconDefinition,C.toHtml,oa=C.icon,C.layer,C.text,C.counter});function Qo(e,t,n){if(!t.autoAddCss||Ze.has(e))return;if(e.getElementById(la)!=null){t.autoAddCss=!1,Ze.add(e);return}let a=e.createElement(`style`);a.setAttribute(`type`,`text/css`),a.setAttribute(`id`,la),n&&a.setAttribute(`nonce`,n),a.innerHTML=ia.css();let i=e.head.childNodes,r=null;for(let o=i.length-1;o>-1;o--){let s=i[o],l=s.nodeName.toUpperCase();[`STYLE`,`LINK`].indexOf(l)>-1&&(r=s)}e.head.insertBefore(a,r),t.autoAddCss=!1,Ze.add(e)}var Vo;var Bo;var Go;var Ko;var fa;var Jo;var Ze;var la;var Zo;var es;var ns;var ts;var Os;var Ds;var as=w(()=>{$M();$M();Xe$1();sa();Vo=(()=>{class e{defaultPrefix=`fas`;fallbackIcon=null;fixedWidth;set autoAddCss(n){aa.autoAddCss=n,this._autoAddCss=n}get autoAddCss(){return this._autoAddCss}_autoAddCss=!0;static ɵfac=function(a){return new(a||e)};static ɵprov=oe$1({token:e,factory:e.ɵfac,providedIn:`root`})}return e})(),Bo=(()=>{class e{definitions={};addIcons(...n){for(let a of n){a.prefix in this.definitions||(this.definitions[a.prefix]={}),this.definitions[a.prefix][a.iconName]=a;for(let i of a.icon[2])typeof i==`string`&&(this.definitions[a.prefix][i]=a)}}addIconPacks(...n){for(let a of n){let i=Object.keys(a).map(r=>a[r]);this.addIcons(...i)}}getIconDefinition(n,a){return n in this.definitions&&a in this.definitions[n]?this.definitions[n][a]:null}static ɵfac=function(a){return new(a||e)};static ɵprov=oe$1({token:e,factory:e.ɵfac,providedIn:`root`})}return e})(),Go=e=>{throw new Error(`Could not find icon with iconName=${e.iconName} and prefix=${e.prefix} in the icon library.`)},Ko=()=>{throw new Error("Property `icon` is required for `fa-icon`/`fa-duotone-icon` components.")},fa=e=>e!=null&&(e===90||e===180||e===270||e===`90`||e===`180`||e===`270`),Jo=e=>{let t=fa(e.rotate),n={[`fa-${e.animation}`]:e.animation!=null&&!e.animation.startsWith(`spin`),"fa-spin":e.animation===`spin`||e.animation===`spin-reverse`,"fa-spin-pulse":e.animation===`spin-pulse`||e.animation===`spin-pulse-reverse`,"fa-spin-reverse":e.animation===`spin-reverse`||e.animation===`spin-pulse-reverse`,"fa-pulse":e.animation===`spin-pulse`||e.animation===`spin-pulse-reverse`,"fa-fw":e.fixedWidth,"fa-border":e.border,"fa-inverse":e.inverse,"fa-layers-counter":e.counter,"fa-flip-horizontal":e.flip===`horizontal`||e.flip===`both`,"fa-flip-vertical":e.flip===`vertical`||e.flip===`both`,[`fa-${e.size}`]:e.size!=null,[`fa-rotate-${e.rotate}`]:t,"fa-rotate-by":e.rotate!=null&&!t,[`fa-pull-${e.pull}`]:e.pull!=null,[`fa-stack-${e.stackItemSize}`]:e.stackItemSize!=null};return Object.keys(n).map(a=>n[a]?a:null).filter(a=>a!=null)},Ze=/* @__PURE__ */ new WeakSet,la=`fa-auto-css`;Zo=e=>e.prefix!==void 0&&e.iconName!==void 0,es=(e,t)=>Zo(e)?e:Array.isArray(e)&&e.length===2?{prefix:e[0],iconName:e[1]}:{prefix:t,iconName:e},ns=(()=>{class e{stackItemSize=SB(`1x`);size=SB();_effect=os(()=>{if(this.size())throw new Error(`fa-icon is not allowed to customize size when used inside fa-stack. Set size on the enclosing fa-stack instead: <fa-stack size="4x">...</fa-stack>.`)});static ɵfac=function(a){return new(a||e)};static ɵdir=jC({type:e,selectors:[[`fa-icon`,`stackItemSize`,``],[`fa-duotone-icon`,`stackItemSize`,``]],inputs:{stackItemSize:[1,`stackItemSize`],size:[1,`size`]}})}return e})(),ts=(()=>{class e{size=SB();classes=ye(()=>{let n=this.size(),a=n?{[`fa-${n}`]:!0}:{};return s(r({},a),{"fa-stack":!0})});static ɵfac=function(a){return new(a||e)};static ɵcmp=(function(){return OC({type:e,selectors:[[`fa-stack`]],hostVars:2,hostBindings:function(i,r){i&2&&b_(r.classes())},inputs:{size:[1,`size`]},ngContentSelectors:[`*`],decls:1,vars:0,template:function(i,r){i&1&&(n_(),r_(0))},encapsulation:2})})()}return e})(),Os=(()=>{class e{icon=xB();title=xB();animation=xB();mask=xB();flip=xB();size=xB();pull=xB();border=xB();inverse=xB();symbol=xB();rotate=xB();fixedWidth=xB();transform=xB();a11yRole=xB();renderedIconHTML=ye(()=>{let n=this.icon()??this.config.fallbackIcon;if(!n)return Ko(),``;let a=this.findIconDefinition(n);if(!a)return``;let i=this.buildParams();Qo(this.document,this.config,this.cspNonce);let r=oa(a,i);return this.sanitizer.bypassSecurityTrustHtml(r.html.join(`
`))});cspNonce=D$1($I);document=D$1(cr$1);sanitizer=D$1(We$1);config=D$1(Vo);iconLibrary=D$1(Bo);stackItem=D$1(ns,{optional:!0});stack=D$1(ts,{optional:!0});constructor(){this.stack!=null&&this.stackItem==null&&console.error(`FontAwesome: fa-icon and fa-duotone-icon elements must specify stackItemSize attribute when wrapped into fa-stack. Example: <fa-icon stackItemSize="2x" />.`)}findIconDefinition(n){let a=es(n,this.config.defaultPrefix);if(`icon`in a)return a;return this.iconLibrary.getIconDefinition(a.prefix,a.iconName)??(Go(a),null)}buildParams(){let n=this.fixedWidth(),a={flip:this.flip(),animation:this.animation(),border:this.border(),inverse:this.inverse(),size:this.size(),pull:this.pull(),rotate:this.rotate(),fixedWidth:typeof n==`boolean`?n:this.config.fixedWidth,stackItemSize:this.stackItem!=null?this.stackItem.stackItemSize():void 0},i=this.transform(),r=typeof i==`string`?ra.transform(i):i,o=this.mask(),s=o!=null?this.findIconDefinition(o):null,l={},u=this.a11yRole();u!=null&&(l.role=u);let d={};return a.rotate!=null&&!fa(a.rotate)&&(d[`--fa-rotate-angle`]=`${a.rotate}`),{title:this.title(),transform:r,classes:Jo(a),mask:s??void 0,symbol:this.symbol(),attributes:l,styles:d}}static ɵfac=function(a){return new(a||e)};static ɵcmp=OC({type:e,selectors:[[`fa-icon`]],hostAttrs:[1,`ng-fa-icon`],hostVars:2,hostBindings:function(a,i){a&2&&(rv(`innerHTML`,i.renderedIconHTML(),vw),Qy(`title`,i.title()??void 0))},inputs:{icon:[1,`icon`],title:[1,`title`],animation:[1,`animation`],mask:[1,`mask`],flip:[1,`flip`],size:[1,`size`],pull:[1,`pull`],border:[1,`border`],inverse:[1,`inverse`],symbol:[1,`symbol`],rotate:[1,`rotate`],fixedWidth:[1,`fixedWidth`],transform:[1,`transform`],a11yRole:[1,`a11yRole`]},outputs:{icon:`iconChange`,title:`titleChange`,animation:`animationChange`,mask:`maskChange`,flip:`flipChange`,size:`sizeChange`,pull:`pullChange`,border:`borderChange`,inverse:`inverseChange`,symbol:`symbolChange`,rotate:`rotateChange`,fixedWidth:`fixedWidthChange`,transform:`transformChange`,a11yRole:`a11yRoleChange`},decls:0,vars:0,template:function(a,i){},encapsulation:2})}return e})(),Ds=(()=>{class e{static ɵfac=function(a){return new(a||e)};static ɵmod=LC({type:e});static ɵinj=$p({})}return e})()});export{as as i,Ds as n,Os as r,Bo as t};