// Agent Builder Canvas — vanilla JS module (no framework deps)
'use strict';

const instances = new Map();

const SIZES = {
  supervisor: { w: 304, h: 112 },
  sub:        { w: 240, h: 96  },
  tool:       { w: 216, h: 46  },
};

const QUESTIONS = [
  { text: "Where should billing answers come from?",  options: ["Knowledge base", "Live billing system", "A human agent"] },
  { text: "Can Atlas issue refunds on its own?",      options: ["Yes, up to a limit", "Only with approval", "Never"] },
  { text: "Which channels should Atlas cover?",       options: ["WhatsApp", "Instagram", "Email", "All of them"] },
];

const SVG_SUB = {
  billing: `<svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="#2A7DE1" stroke-width="1.8"><rect x="3" y="6" width="18" height="12" rx="2"/><path d="M3 10h18" stroke-linecap="round"/></svg>`,
  support: `<svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="#2A7DE1" stroke-width="1.8"><path d="M21 11.5a8.4 8.4 0 0 1-9 8.3L3 21l1.3-4.2A8.5 8.5 0 1 1 21 11.5z" stroke-linejoin="round"/></svg>`,
  generic: `<svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="#2A7DE1" stroke-width="1.8"><circle cx="12" cy="12" r="3"/><path d="M12 3v3M12 18v3M3 12h3M18 12h3" stroke-linecap="round"/></svg>`,
};

const SVG_TOOL  = `<svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="#5C6470" stroke-width="1.9"><path d="M14.5 5.5a4 4 0 0 1-5 5L4 16v4h4l5.5-5.5a4 4 0 0 0 5-5l-2.6 2.6-2.4-.6-.6-2.4 2.6-2.6z" stroke-linejoin="round"/></svg>`;
const SVG_WARN  = `<svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2"><path d="M12 8v5M12 16.5v.5" stroke-linecap="round"/><path d="M10.3 3.3 2.6 17a2 2 0 0 0 1.7 3h15.4a2 2 0 0 0 1.7-3L13.7 3.3a2 2 0 0 0-3.4 0z"/></svg>`;
const SVG_CLOSE = `<svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.6"><path d="M6 6l12 12M18 6L6 18" stroke-linecap="round"/></svg>`;
const SVG_DEL   = `<svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9"><path d="M5 7h14M9 7V5h6v2M7 7l1 13h8l1-13" stroke-linecap="round" stroke-linejoin="round"/></svg>`;

function pipSvg(pose) {
  const cls = `abc-pip-svg abc-pip-svg--${pose}`;
  return `<svg class="${cls}" viewBox="0 0 100 120" xmlns="http://www.w3.org/2000/svg">
    <path d="M30 95 Q22 112 36 118 Q50 112 64 118 Q78 112 70 95" fill="#4C1D95"/>
    <ellipse cx="50" cy="82" rx="28" ry="24" fill="#6D28D9"/>
    <path d="M24 74 Q12 90 24 106 Q32 90 40 82" fill="#5B21B6"/>
    <path d="M76 74 Q88 90 76 106 Q68 90 60 82" fill="#5B21B6"/>
    <circle cx="50" cy="44" r="26" fill="#7C3AED"/>
    <ellipse cx="45" cy="51" rx="15" ry="12" fill="#F5F0FF"/>
    <path d="M38 51 L28 57 L39 60 Z" fill="#D97706"/>
    <circle cx="42" cy="42" r="8" fill="white"/>
    <circle cx="43" cy="42" r="5" fill="#1F2937"/>
    <circle cx="41.5" cy="40.5" r="1.8" fill="white"/>
    <ellipse cx="57" cy="51" rx="9" ry="6" fill="rgba(251,191,36,.35)"/>
    <path d="M44 20 Q42 11 47 7 Q53 12 51 22" fill="#8B5CF6"/>
    <path d="M52 18 Q52 9 57 5 Q62 11 58 22" fill="#A78BFA"/>
    <path d="M42 106 L38 114 M42 106 L44 114 M42 106 L47 112" stroke="#7C3AED" stroke-width="2.2" stroke-linecap="round" fill="none"/>
    <path d="M58 106 L54 114 M58 106 L60 114 M58 106 L63 112" stroke="#7C3AED" stroke-width="2.2" stroke-linecap="round" fill="none"/>
  </svg>`;
}

// ─────────────────────────────────────────────
//  Instance factory
// ─────────────────────────────────────────────
function createInstance(containerId) {
  const container = document.getElementById(containerId);
  if (!container) return null;

  // ── state ──
  const st = {
    nodes: [], edges: [], selected: null, hovered: null,
    drag: null,   // { id, ox, oy }
    conn: null,   // { from, x, y, over }
    parrot: 'greet', parrotMin: false,
    bubble: { show: true, kind: 'greet',
      text: "Hi, I'm Pip — your build guide. Drop a Supervisor on the canvas and I'll walk you through the rest.",
      options: [] },
    toast: null,
    qIndex: -1,
    uid: 1,
    transient: false,
  };
  const timers = {};

  // ── DOM ──
  container.innerHTML = `
    <div class="abc-cv" id="${containerId}-cv" touch-action="none">
      <svg class="abc-cv-edges" xmlns="http://www.w3.org/2000/svg" overflow="visible">
        <path class="abc-temp-edge" fill="none" stroke="#9B7FD4" stroke-width="2.2"
              stroke-dasharray="6 6" stroke-linecap="round"/>
      </svg>
      <div class="abc-cv-nodes"></div>

      <div class="abc-cv-empty" style="display:none;">
        <div class="abc-cv-empty-card">
          <div class="abc-cv-empty-icon">
            <svg width="30" height="30" viewBox="0 0 24 24" fill="none" stroke="#53279E" stroke-width="1.6">
              <rect x="4" y="8" width="16" height="11" rx="3"/>
              <circle cx="9" cy="13.5" r="1.4" fill="#53279E" stroke="none"/>
              <circle cx="15" cy="13.5" r="1.4" fill="#53279E" stroke="none"/>
              <path d="M12 8V4M9 4h6" stroke-linecap="round"/>
            </svg>
          </div>
          <div class="abc-cv-empty-title">Start with a Supervisor</div>
          <p class="abc-cv-empty-sub">The Supervisor routes every conversation to the right sub-agent. Add it first, then attach sub-agents and tools.</p>
          <div class="abc-cv-empty-btns">
            <button class="abc-btn abc-btn--primary abc-add-sup-empty">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2"><path d="M12 5v14M5 12h14" stroke-linecap="round"/></svg>
              Add Supervisor
            </button>
            <button class="abc-btn abc-btn--ghost abc-seed-empty">Load sample</button>
          </div>
        </div>
      </div>

      <div class="abc-palette">
        <button class="abc-palette-btn abc-palette-btn--sup abc-add-sup">
          <span class="abc-pdot" style="background:#53279E;border-radius:3px;width:9px;height:9px;display:inline-block;flex-shrink:0;"></span>Supervisor
        </button>
        <button class="abc-palette-btn abc-add-sub">
          <span class="abc-pdot" style="background:#2A7DE1;border-radius:3px;width:9px;height:9px;display:inline-block;flex-shrink:0;"></span>Sub-agent
        </button>
        <button class="abc-palette-btn abc-add-tool">
          <span class="abc-pdot" style="background:#5C6470;border-radius:50%;width:9px;height:9px;display:inline-block;flex-shrink:0;"></span>Tool
        </button>
        <span class="abc-psep"></span>
        <button class="abc-palette-icon abc-seed-btn" title="Load sample">
          <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7"><path d="M4 4v6h6M20 20v-6h-6" stroke-linecap="round" stroke-linejoin="round"/><path d="M5 10a8 8 0 0 1 14-2M19 14a8 8 0 0 1-14 2" stroke-linecap="round"/></svg>
        </button>
        <button class="abc-palette-icon abc-clear-btn" title="Clear canvas">
          <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7"><path d="M5 7h14M9 7V5h6v2M7 7l1 13h8l1-13" stroke-linecap="round" stroke-linejoin="round"/></svg>
        </button>
      </div>

      <div class="abc-hint" style="display:none;">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="#9B7FD4" stroke-width="2"><circle cx="6" cy="12" r="2.5"/><circle cx="18" cy="6" r="2.5"/><path d="M8.2 11l7.6-3.8"/></svg>
        Drag from a node's bottom dot to another node to connect them
      </div>

      <div class="abc-inspector" style="display:none;"></div>

      <div class="abc-pip-wrap">
        <div class="abc-bubble-wrap" style="display:none;"></div>
        <div class="abc-pip-row">
          <button class="abc-ask-btn">
            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M9.5 9a2.5 2.5 0 1 1 3.4 2.3c-.8.3-1.4 1-1.4 1.9v.3M12 17v.3" stroke-linecap="round"/><circle cx="12" cy="12" r="9.2"/></svg>
            Ask Pip
          </button>
          <div class="abc-pip-fig-wrap">
            <div class="abc-pip-fig"></div>
            <button class="abc-pip-min" title="Minimise Pip">
              <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.6"><path d="M5 12h14" stroke-linecap="round"/></svg>
            </button>
          </div>
        </div>
      </div>

      <div class="abc-pip-peek" style="display:none;" title="Bring Pip back"></div>

      <div class="abc-toast" style="display:none;"></div>
    </div>`;

  const cv        = container.querySelector('.abc-cv');
  const nodesEl   = container.querySelector('.abc-cv-nodes');
  const edgesSvg  = container.querySelector('.abc-cv-edges');
  const tempEdge  = container.querySelector('.abc-temp-edge');
  const emptyEl   = container.querySelector('.abc-cv-empty');
  const hintEl    = container.querySelector('.abc-hint');
  const inspEl    = container.querySelector('.abc-inspector');
  const pipWrap   = container.querySelector('.abc-pip-wrap');
  const pipFig    = container.querySelector('.abc-pip-fig');
  const pipPeek   = container.querySelector('.abc-pip-peek');
  const bubbleWrap= container.querySelector('.abc-bubble-wrap');
  const toastEl   = container.querySelector('.abc-toast');

  const nodeEls   = new Map(); // id → element
  const edgeEls   = new Map(); // id → { path, dot }

  // ── helpers ──
  function pt(e) {
    const r = cv.getBoundingClientRect();
    return { x: e.clientX - r.left, y: e.clientY - r.top };
  }
  const nodeById = id => st.nodes.find(n => n.id === id);
  function anchorPt(n) { return { x: n.x + n.w / 2, y: n.y + n.h }; }
  function topPt(n)    { return { x: n.x + n.w / 2, y: n.y }; }
  function edgePath(a, b) {
    const s = anchorPt(a), t = topPt(b);
    const dy = Math.max(34, Math.abs(t.y - s.y) * 0.5);
    return `M${s.x},${s.y} C${s.x},${s.y + dy} ${t.x},${t.y - dy} ${t.x},${t.y}`;
  }
  function genId(p) { return p + (st.uid++); }

  // ── parrot ──
  function setParrot(pose, ms, transient) {
    clearTimeout(timers.parrot);
    if (transient) st.transient = true;
    st.parrot = pose;
    renderPip();
    if (ms) timers.parrot = setTimeout(() => {
      st.transient = false;
      st.parrot = restPose();
      renderPip();
    }, ms);
  }
  function restPose() {
    if (!st.nodes.length && st.bubble.show && st.bubble.kind === 'greet') return 'greet';
    return st.hovered ? 'curious' : 'idle';
  }

  // ── toast ──
  function toast(text, kind) {
    clearTimeout(timers.toast);
    st.toast = { text, kind: kind || 'ok' };
    renderToast();
    timers.toast = setTimeout(() => { st.toast = null; renderToast(); }, 2200);
  }

  // ── node styling ──
  function nodeBoxStyle(n) {
    const sel = st.selected === n.id;
    const hov = st.hovered === n.id && !sel;
    const drag = st.drag && st.drag.id === n.id;
    const connOver = st.conn && st.conn.over === n.id;
    const base = n.type === 'supervisor'
      ? { bg: '#FCFBFF', br: '16px', pad: '15px 17px', bc: '#E7DBF7', hbc: '#D6C9F2' }
      : n.type === 'sub'
      ? { bg: '#FFFFFF', br: '14px', pad: '13px 15px', bc: '#E8E8EB', hbc: '#BFD6F5' }
      : { bg: '#FFFFFF', br: '24px', pad: '0 12px', bc: '#E8E8EB', hbc: '#CBD2DB' };

    let border = base.bc;
    let shadow = '0 1px 2px rgba(31,41,55,.05)';
    let tx = '';

    if (hov)     { border = base.hbc; shadow = '0 9px 22px rgba(31,41,55,.11)'; tx = 'translateY(-2px)'; }
    if (sel)     { border = '#53279E'; shadow = '0 0 0 4px rgba(83,39,158,.12),0 9px 22px rgba(31,41,55,.12)'; tx = hov ? 'translateY(-2px)' : ''; }
    if (n.invalid) { border = '#D60032'; shadow = '0 0 0 4px rgba(214,0,50,.12),0 9px 22px rgba(31,41,55,.10)'; }
    if (connOver)  { border = '#2A7DE1'; shadow = '0 0 0 4px rgba(42,125,225,.16)'; tx = 'translateY(-1px)'; }

    const bw = (sel || n.invalid) ? '2px' : '1.5px';
    const anim = n.entering ? 'abc-nodeIn .26s cubic-bezier(.2,.7,.3,1.2)' : n.leaving ? 'abc-nodeOut .22s ease-in forwards' : 'none';
    const zIndex = drag ? 50 : sel ? 30 : hov ? 20 : 10;
    const cursor  = drag ? 'grabbing' : 'grab';
    const h = n.type === 'tool' ? `height:${n.h}px;display:flex;align-items:center;gap:9px;` : '';

    return `position:absolute;left:${n.x}px;top:${n.y}px;width:${n.w}px;box-sizing:border-box;`
      + `user-select:none;cursor:${cursor};transition:box-shadow .18s,transform .18s,border-color .18s;`
      + `z-index:${zIndex};background:${base.bg};border-radius:${base.br};padding:${base.pad};`
      + `border:${bw} solid ${border};box-shadow:${shadow};`
      + (tx ? `transform:${tx};` : '')
      + (anim !== 'none' ? `animation:${anim};` : '')
      + h;
  }

  function badgeStyle(risk) {
    if (risk === 'high') return 'background:#FDECEF;color:#C8102E;border:1px solid #F6CDD6;';
    if (risk === 'low')  return 'background:#FEF3E2;color:#B45309;border:1px solid #F6E0B8;';
    return 'background:#EEF1F5;color:#545B62;border:1px solid #E2E4E9;';
  }
  function badgeLabel(risk) {
    return risk === 'high' ? 'ACTION · HIGH' : risk === 'low' ? 'ACTION · LOW' : 'READ';
  }

  // ── render nodes ──
  function renderNodes() {
    // Remove leaving nodes after animation
    for (const [id, el] of nodeEls) {
      if (!nodeById(id)) { el.remove(); nodeEls.delete(id); }
    }
    for (const n of st.nodes) {
      let el = nodeEls.get(n.id);
      if (!el) {
        el = document.createElement('div');
        el.dataset.nodeId = n.id;
        nodesEl.appendChild(el);
        nodeEls.set(n.id, el);
      }
      el.style.cssText = nodeBoxStyle(n);
      el.innerHTML = nodeInnerHtml(n);
    }
  }

  function nodeInnerHtml(n) {
    if (n.type === 'supervisor') return `
      <div style="display:flex;align-items:center;gap:11px;">
        <div style="width:36px;height:36px;border-radius:11px;background:#53279E;display:flex;align-items:center;justify-content:center;flex-shrink:0;box-shadow:0 4px 10px rgba(83,39,158,.25);">
          <svg width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="1.8"><path d="M12 3l2 5 5 .4-3.8 3.3L16.5 17 12 14.2 7.5 17l1.3-5.3L5 8.4 10 8l2-5z" stroke-linejoin="round"/></svg>
        </div>
        <div style="display:flex;flex-direction:column;min-width:0;">
          <span style="font-size:10px;font-weight:800;letter-spacing:.09em;color:#8B7BB5;">SUPERVISOR</span>
          <span style="font-size:15.5px;font-weight:700;letter-spacing:-.01em;">${esc(n.title)}</span>
        </div>
        ${n.invalid ? `<div style="margin-left:auto;display:flex;align-items:center;gap:5px;background:#FDECEF;color:#C8102E;font-size:10.5px;font-weight:700;padding:3px 8px;border-radius:20px;">${SVG_WARN}${esc(n.warn)}</div>` : ''}
      </div>
      <span style="font-size:12.5px;color:#6B7280;line-height:1.45;margin-top:9px;display:block;">${esc(n.subtitle)}</span>
      <div data-anchor-id="${n.id}" style="position:absolute;bottom:-9px;left:50%;transform:translateX(-50%);width:18px;height:18px;border-radius:50%;background:#fff;border:2.5px solid #53279E;cursor:crosshair;z-index:3;display:flex;align-items:center;justify-content:center;"><span style="width:5px;height:5px;border-radius:50%;background:#53279E;pointer-events:none;"></span></div>`;

    if (n.type === 'sub') return `
      <div style="display:flex;align-items:center;gap:10px;">
        <div style="width:30px;height:30px;border-radius:9px;background:#EAF2FD;display:flex;align-items:center;justify-content:center;flex-shrink:0;">
          ${SVG_SUB[n.iconKey] || SVG_SUB.generic}
        </div>
        <div style="display:flex;flex-direction:column;min-width:0;">
          <span style="font-size:9.5px;font-weight:800;letter-spacing:.08em;color:#A8AEB8;">SUB-AGENT</span>
          <span style="font-size:14px;font-weight:700;letter-spacing:-.01em;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;">${esc(n.title)}</span>
        </div>
        ${n.invalid ? `<div style="margin-left:auto;display:flex;align-items:center;gap:4px;background:#FDECEF;color:#C8102E;font-size:9.5px;font-weight:700;padding:3px 7px;border-radius:20px;white-space:nowrap;">${SVG_WARN}${esc(n.warn)}</div>` : ''}
      </div>
      <span style="font-size:11.5px;color:#7A828E;line-height:1.4;margin-top:7px;display:block;">${esc(n.subtitle)}</span>
      <div data-anchor-id="${n.id}" style="position:absolute;bottom:-8px;left:50%;transform:translateX(-50%);width:16px;height:16px;border-radius:50%;background:#fff;border:2.5px solid #2A7DE1;cursor:crosshair;z-index:3;display:flex;align-items:center;justify-content:center;"><span style="width:4px;height:4px;border-radius:50%;background:#2A7DE1;pointer-events:none;"></span></div>`;

    // tool
    const lk = n.risk === 'high' ? `<svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4"><rect x="5" y="11" width="14" height="9" rx="2"/><path d="M8 11V8a4 4 0 0 1 8 0v3" stroke-linecap="round"/></svg>` : '';
    return `
      <div style="width:22px;height:22px;border-radius:7px;background:#F1F2F4;display:flex;align-items:center;justify-content:center;flex-shrink:0;">${SVG_TOOL}</div>
      <span style="font-size:12.5px;font-weight:600;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;flex:1;">${esc(n.title)}</span>
      <span style="display:inline-flex;align-items:center;gap:4px;font-size:9.5px;font-weight:800;letter-spacing:.03em;padding:3px 8px;border-radius:20px;flex-shrink:0;white-space:nowrap;${badgeStyle(n.risk)}">${lk}${badgeLabel(n.risk)}</span>`;
  }

  // ── render edges ──
  function renderEdges() {
    for (const [id, els] of edgeEls) {
      if (!st.edges.find(e => e.id === id)) {
        els.path.remove(); els.dot.remove(); edgeEls.delete(id);
      }
    }
    const nMap = Object.fromEntries(st.nodes.map(n => [n.id, n]));
    for (const e of st.edges) {
      const a = nMap[e.from], b = nMap[e.to];
      if (!a || !b) continue;
      let els = edgeEls.get(e.id);
      if (!els) {
        const path = document.createElementNS('http://www.w3.org/2000/svg', 'path');
        path.setAttribute('fill', 'none');
        path.setAttribute('stroke-linecap', 'round');
        path.setAttribute('pathLength', '1');
        const dot = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
        dot.setAttribute('r', '3.4');
        dot.setAttribute('fill', '#fff');
        dot.setAttribute('stroke-width', '2');
        if (e.drawing) {
          path.style.cssText = 'stroke-dasharray:1;stroke-dashoffset:1;animation:abc-edgeDraw .44s ease-out forwards;';
        }
        edgesSvg.insertBefore(path, tempEdge);
        edgesSvg.insertBefore(dot, tempEdge);
        els = { path, dot }; edgeEls.set(e.id, els);
      }
      const isSel = st.selected && (e.from === st.selected || e.to === st.selected);
      const stroke = isSel ? '#9B7FD4' : '#C7C0D8';
      const width  = isSel ? '2.6' : '2';
      els.path.setAttribute('d', edgePath(a, b));
      els.path.setAttribute('stroke', stroke);
      els.path.setAttribute('stroke-width', width);
      const t = topPt(b);
      els.dot.setAttribute('cx', t.x);
      els.dot.setAttribute('cy', t.y);
      els.dot.setAttribute('stroke', stroke);
    }

    // temp edge
    if (st.conn && nMap[st.conn.from]) {
      const a = anchorPt(nMap[st.conn.from]);
      const dy = Math.max(30, Math.abs(st.conn.y - a.y) * 0.5);
      tempEdge.setAttribute('d', `M${a.x},${a.y} C${a.x},${a.y + dy} ${st.conn.x},${st.conn.y - dy} ${st.conn.x},${st.conn.y}`);
    } else {
      tempEdge.setAttribute('d', '');
    }
  }

  // ── render empty / hint ──
  function renderMeta() {
    emptyEl.style.display = !st.nodes.length ? 'flex' : 'none';
    hintEl.style.display  = (st.nodes.length > 0 && !st.edges.length) ? 'flex' : 'none';
  }

  // ── render inspector ──
  function renderInspector() {
    const n = st.selected ? nodeById(st.selected) : null;
    if (!n) { inspEl.style.display = 'none'; return; }
    inspEl.style.display = 'block';

    const kindLabel = n.type === 'supervisor' ? 'Supervisor' : n.type === 'sub' ? 'Sub-agent' : 'Tool';
    const chipColor = n.type === 'supervisor' ? '#53279E' : n.type === 'sub' ? '#2A7DE1' : '#5C6470';
    const chipBg    = n.type === 'supervisor' ? '#F3EEFC' : n.type === 'sub' ? '#EAF2FD'  : '#F1F2F4';
    const subtitle  = n.type === 'tool' ? `This tool is currently a ${badgeLabel(n.risk)} action.` : n.subtitle;

    const riskBtnHtml = (risk, label, activeColor, activeBg, activeBorder, dotColor) => {
      const active = n.risk === risk;
      const style = active
        ? `color:${activeColor};background:${activeBg};border-color:${activeBorder};`
        : 'color:#3A4150;background:#FFFFFF;border-color:#EAEAEE;';
      return `<button data-set-risk="${risk}" style="display:flex;align-items:center;gap:9px;width:100%;height:36px;padding:0 12px;border-radius:9px;font-size:12.5px;font-weight:600;cursor:pointer;text-align:left;border:1px solid;${style}">
        <span style="width:8px;height:8px;border-radius:50%;background:${dotColor};flex-shrink:0;"></span>${label}
      </button>`;
    };

    inspEl.innerHTML = `
      <div style="padding:15px 16px 13px;border-bottom:1px solid #F0F0F2;display:flex;align-items:center;gap:9px;">
        <span style="font-size:10.5px;font-weight:800;letter-spacing:.05em;color:${chipColor};background:${chipBg};padding:4px 10px;border-radius:20px;text-transform:uppercase;">${kindLabel}</span>
        <button data-insp-close style="margin-left:auto;width:27px;height:27px;border-radius:7px;border:none;background:#F4F4F6;color:#6B7280;cursor:pointer;display:flex;align-items:center;justify-content:center;">${SVG_CLOSE}</button>
      </div>
      <div style="padding:16px;">
        <div style="font-size:16px;font-weight:700;letter-spacing:-.01em;">${esc(n.title)}</div>
        <div style="font-size:12.5px;color:#6B7280;line-height:1.5;margin-top:6px;">${esc(subtitle)}</div>
        ${n.type === 'tool' ? `
        <div style="margin-top:17px;">
          <div style="font-size:10.5px;font-weight:700;letter-spacing:.06em;color:#AEB3BC;margin-bottom:9px;">RISK LEVEL</div>
          <div style="display:flex;flex-direction:column;gap:7px;">
            ${riskBtnHtml('read', 'Read · safe lookups',   '#3A4150','#F6F7F8','#DDE0E5','#5C6470')}
            ${riskBtnHtml('low',  'Action · low risk',     '#B45309','#FEF7EC','#F6E0B8','#E0890B')}
            ${riskBtnHtml('high', 'Action · high risk',    '#C8102E','#FEF1F4','#F6CDD6','#C8102E')}
          </div>
        </div>` : ''}
        <div style="margin-top:17px;padding-top:15px;border-top:1px solid #F0F0F2;display:flex;flex-direction:column;gap:10px;">
          <div style="display:flex;align-items:center;gap:8px;font-size:12px;color:#6B7280;">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="#9AA0AA" stroke-width="1.7"><circle cx="12" cy="12" r="9"/><path d="M12 8v4l3 2" stroke-linecap="round"/></svg>Edited just now
          </div>
          <div style="display:flex;align-items:center;gap:8px;font-size:12px;color:#6B7280;">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="#9AA0AA" stroke-width="1.7"><path d="M6 12h2.5l2-6 3 14 2.5-8H18" stroke-linecap="round" stroke-linejoin="round"/></svg>Drag the dot below to connect
          </div>
        </div>
        <button data-insp-del style="margin-top:17px;width:100%;height:39px;border-radius:10px;border:1px solid #F6CDD6;background:#FEF6F8;color:#C8102E;font-size:12.5px;font-weight:600;cursor:pointer;display:flex;align-items:center;justify-content:center;gap:8px;">${SVG_DEL}Remove node</button>
      </div>`;

    inspEl.querySelector('[data-insp-close]')?.addEventListener('click', () => { st.selected = null; renderInspector(); renderNodes(); });
    inspEl.querySelector('[data-insp-del]')?.addEventListener('click', () => deleteNode(st.selected));
    inspEl.querySelectorAll('[data-set-risk]').forEach(btn => {
      btn.addEventListener('click', () => {
        const n2 = nodeById(st.selected);
        if (n2) { n2.risk = btn.dataset.setRisk; n2.invalid = false; n2.warn = null; }
        renderNodes(); renderInspector();
      });
    });
  }

  // ── render pip/bubble ──
  function renderPip() {
    if (st.parrotMin) {
      pipWrap.style.display = 'none';
      pipPeek.style.display = 'block';
      pipPeek.innerHTML = pipSvg('curious');
    } else {
      pipWrap.style.display = 'flex';
      pipPeek.style.display = 'none';
      pipFig.innerHTML = pipSvg(st.parrot);
    }

    // bubble
    if (st.bubble.show) {
      bubbleWrap.style.display = 'block';
      const optsHtml = (st.bubble.kind === 'question' && st.bubble.options.length)
        ? `<div style="display:flex;flex-wrap:wrap;gap:7px;margin-top:13px;">
            ${st.bubble.options.map((o, i) => `<button data-opt="${i}" style="font-size:12px;font-weight:600;color:#53279E;background:#F6F2FE;border:1px solid #E7DEFA;padding:7px 12px;border-radius:9px;cursor:pointer;">${esc(o)}</button>`).join('')}
           </div>` : '';
      const prog = (st.bubble.kind === 'question' && st.qIndex >= 0)
        ? `<span style="font-size:9.5px;font-weight:700;color:#8B7BB5;background:#F3EEFC;padding:2px 7px;border-radius:20px;">Question ${st.qIndex + 1} of ${QUESTIONS.length}</span>` : '';
      bubbleWrap.innerHTML = `
        <div style="max-width:320px;background:#FFFFFF;border:1px solid #EBE7F5;border-radius:16px 16px 4px 16px;box-shadow:0 12px 30px rgba(83,39,158,.16);padding:15px 17px;animation:abc-bubbleIn .32s cubic-bezier(.2,.8,.3,1.2);">
          <div style="display:flex;align-items:center;gap:7px;margin-bottom:8px;">
            <span style="font-size:11px;font-weight:800;color:#53279E;letter-spacing:.02em;">PIP</span>
            ${prog}
            <button data-dismiss style="margin-left:auto;width:22px;height:22px;border-radius:6px;border:none;background:#F4F4F6;color:#9AA0AA;cursor:pointer;display:flex;align-items:center;justify-content:center;">${SVG_CLOSE}</button>
          </div>
          <div style="font-size:14px;font-weight:600;color:#26282E;line-height:1.45;">${esc(st.bubble.text)}</div>
          ${optsHtml}
        </div>`;
      bubbleWrap.querySelector('[data-dismiss]')?.addEventListener('click', dismissBubble);
      bubbleWrap.querySelectorAll('[data-opt]').forEach(btn => {
        btn.addEventListener('click', () => pickOption(st.bubble.options[+btn.dataset.opt]));
      });
    } else {
      bubbleWrap.style.display = 'none';
    }
  }

  // ── render toast ──
  function renderToast() {
    if (!st.toast) { toastEl.style.display = 'none'; return; }
    const warn = st.toast.kind === 'warn';
    const icon = warn
      ? `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="2.2"><path d="M12 8v5M12 16.5v.5" stroke-linecap="round"/><circle cx="12" cy="12" r="9"/></svg>`
      : `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="2.4"><path d="M5 12l5 5L20 7" stroke-linecap="round" stroke-linejoin="round"/></svg>`;
    toastEl.style.cssText = `display:flex;align-items:center;gap:9px;background:${warn ? '#B45309' : '#1F2937'};color:#fff;font-size:13px;font-weight:600;padding:11px 17px;border-radius:11px;box-shadow:0 10px 26px rgba(31,41,55,.18);animation:abc-toastIn .3s ease;`;
    toastEl.innerHTML = icon + esc(st.toast.text);
  }

  function render() { renderNodes(); renderEdges(); renderMeta(); renderInspector(); renderPip(); renderToast(); }

  // ── pointer events ──
  function onDown(e) {
    const anchor = e.target.closest('[data-anchor-id]');
    if (anchor) {
      const id = anchor.getAttribute('data-anchor-id');
      const p = pt(e);
      cv.setPointerCapture(e.pointerId);
      st.conn = { from: id, x: p.x, y: p.y, over: null };
      if (!st.transient) setParrot('curious');
      renderEdges();
      return;
    }
    const nodeEl = e.target.closest('[data-node-id]');
    if (nodeEl) {
      const id = nodeEl.getAttribute('data-node-id');
      const n = nodeById(id); if (!n) return;
      const p = pt(e);
      cv.setPointerCapture(e.pointerId);
      st.selected = id;
      st.drag = { id, ox: p.x - n.x, oy: p.y - n.y };
      if (!st.transient) setParrot('curious');
      renderNodes(); renderEdges(); renderInspector();
      return;
    }
    if (e.target === cv || e.target.closest('.abc-cv-edges') || e.target.closest('.abc-cv-nodes')) {
      st.selected = null; renderNodes(); renderInspector();
    }
  }

  function onMove(e) {
    if (st.drag) {
      const p = pt(e);
      const n = nodeById(st.drag.id); if (!n) return;
      n.x = Math.max(0, p.x - st.drag.ox);
      n.y = Math.max(0, p.y - st.drag.oy);
      const el = nodeEls.get(n.id);
      if (el) { el.style.left = n.x + 'px'; el.style.top = n.y + 'px'; }
      renderEdges();
      return;
    }
    if (st.conn) {
      const p = pt(e);
      const overEl = document.elementFromPoint(e.clientX, e.clientY);
      const overNode = overEl?.closest('[data-node-id]');
      const overId = overNode ? overNode.getAttribute('data-node-id') : null;
      st.conn = { ...st.conn, x: p.x, y: p.y, over: (overId && overId !== st.conn.from) ? overId : null };
      renderEdges();
      if (st.conn.over) {
        const oEl = nodeEls.get(st.conn.over);
        if (oEl) oEl.style.cssText = nodeBoxStyle(nodeById(st.conn.over));
      }
    }
  }

  function onUp(e) {
    if (st.conn) {
      if (st.conn.over) createEdge(st.conn.from, st.conn.over);
      st.conn = null; renderEdges();
    }
    if (st.drag) { st.drag = null; renderNodes(); }
  }

  function onOver(e) {
    const nodeEl = e.target.closest('[data-node-id]');
    if (nodeEl) {
      const id = nodeEl.getAttribute('data-node-id');
      if (id !== st.hovered) {
        st.hovered = id;
        nodeEls.get(id) && (nodeEls.get(id).style.cssText = nodeBoxStyle(nodeById(id)));
        if (!st.transient && !st.drag) setParrot('curious');
      }
    }
  }

  function onOut(e) {
    const to = e.relatedTarget;
    if (!to?.closest?.('[data-node-id]') && st.hovered) {
      const prev = st.hovered; st.hovered = null;
      const el = nodeEls.get(prev); const n = nodeById(prev);
      if (el && n) el.style.cssText = nodeBoxStyle(n);
      if (!st.transient && !st.drag && !st.conn) {
        clearTimeout(timers.rest);
        timers.rest = setTimeout(() => { st.parrot = restPose(); renderPip(); }, 200);
      }
    }
  }

  cv.addEventListener('pointerdown', onDown);
  cv.addEventListener('pointermove', onMove);
  cv.addEventListener('pointerup', onUp);
  cv.addEventListener('pointerover', onOver);
  cv.addEventListener('pointerout', onOut);

  // ── keyboard ──
  const onKey = e => {
    if ((e.key === 'Delete' || e.key === 'Backspace') && st.selected && document.activeElement?.tagName !== 'INPUT') {
      e.preventDefault(); deleteNode(st.selected);
    }
  };
  window.addEventListener('keydown', onKey);

  // ── actions ──
  function createEdge(from, to) {
    const id = `e-${from}-${to}`;
    if (st.edges.some(e => e.id === id || (e.from === to && e.to === from))) { toast('Already connected'); return; }
    st.edges.push({ id, from, to, drawing: true });
    renderEdges();
    setParrot('happy', 1700, true);
    toast('Connected');
    setTimeout(() => { const e = st.edges.find(x => x.id === id); if (e) { e.drawing = false; } }, 480);
  }

  function addNode(type, extra) {
    const sz = SIZES[type] || SIZES.tool;
    const count = st.nodes.filter(n => n.type === type).length;
    const cw = cv.clientWidth || 900;
    let x, y;
    if (type === 'supervisor')     { x = Math.max(40, Math.min(cw / 2 - sz.w / 2, cw - sz.w - 60)); y = 36; }
    else if (type === 'sub')       { x = 60 + (count % 2) * 328;  y = 214 + Math.floor(count / 2) * 36; }
    else                           { x = 60 + (count % 3) * 220;  y = 392; }
    const id = (type[0]) + (st.uid++);
    const node = { id, type, x, y, w: sz.w, h: sz.h, entering: true, invalid: false, warn: null, ...extra };
    st.nodes.push(node);
    st.selected = id;
    if (st.bubble.kind === 'greet') st.bubble.show = false;
    renderNodes(); renderEdges(); renderMeta(); renderInspector();
    setTimeout(() => { node.entering = false; const el = nodeEls.get(id); if (el) el.style.cssText = nodeBoxStyle(node); }, 270);
    return id;
  }

  function addSupervisor() {
    if (st.nodes.some(n => n.type === 'supervisor')) { toast('Only one Supervisor allowed', 'warn'); return; }
    addNode('supervisor', { title: 'Atlas Supervisor', subtitle: 'Routes every conversation to the right sub-agent.' });
  }

  function addSubAgent() {
    const n = st.nodes.filter(x => x.type === 'sub').length;
    const presets = [
      { title: 'Billing Agent',  subtitle: 'Handles invoices, payments and refunds.', iconKey: 'billing' },
      { title: 'Support Agent',  subtitle: 'Answers product questions and troubleshooting.', iconKey: 'support' },
    ];
    const p = presets[n % presets.length] || { title: 'Sub-agent', subtitle: 'Handles a focused set of requests.', iconKey: 'generic' };
    addNode('sub', { title: p.title, subtitle: p.subtitle, iconKey: p.iconKey });
  }

  function addTool() { addNode('tool', { title: 'New tool', risk: 'read' }); }

  function seedSample() {
    let uid = st.uid;
    const mk = (o) => { const id = o.type[0] + uid; uid++; const sz = SIZES[o.type] || SIZES.tool; return { id, w: sz.w, h: sz.h, entering: true, invalid: false, warn: null, ...o }; };
    const sup = mk({ type: 'supervisor', x: 188, y: 36,  title: 'Atlas Supervisor', subtitle: 'Routes every conversation to the right sub-agent.' });
    const a   = mk({ type: 'sub',        x: 60,  y: 214, title: 'Billing Agent',    subtitle: 'Handles invoices, payments and refunds.',           iconKey: 'billing' });
    const b   = mk({ type: 'sub',        x: 388, y: 214, title: 'Support Agent',    subtitle: 'Answers product questions and troubleshooting.',    iconKey: 'support' });
    const t1  = mk({ type: 'tool',       x: 44,  y: 378, title: 'Look up invoice',       risk: 'read' });
    const t2  = mk({ type: 'tool',       x: 44,  y: 434, title: 'Issue refund',           risk: 'high' });
    const t3  = mk({ type: 'tool',       x: 372, y: 378, title: 'Search knowledge base',  risk: 'read' });
    const t4  = mk({ type: 'tool',       x: 372, y: 434, title: 'Create support ticket',  risk: 'low'  });
    st.nodes = [sup, a, b, t1, t2, t3, t4]; st.uid = uid;
    st.edges = [
      { id: `e-${sup.id}-${a.id}`,  from: sup.id, to: a.id  },
      { id: `e-${sup.id}-${b.id}`,  from: sup.id, to: b.id  },
      { id: `e-${a.id}-${t1.id}`,   from: a.id,   to: t1.id },
      { id: `e-${a.id}-${t2.id}`,   from: a.id,   to: t2.id },
      { id: `e-${b.id}-${t3.id}`,   from: b.id,   to: t3.id },
      { id: `e-${b.id}-${t4.id}`,   from: b.id,   to: t4.id },
    ];
    st.selected = a.id; st.parrot = 'idle'; st.bubble = { show: false, kind: 'greet', text: '', options: [] };
    nodeEls.clear(); edgeEls.forEach(els => { els.path.remove(); els.dot.remove(); }); edgeEls.clear();
    nodesEl.innerHTML = '';
    render();
    setTimeout(() => st.nodes.forEach(n => { n.entering = false; const el = nodeEls.get(n.id); if (el) el.style.cssText = nodeBoxStyle(n); }), 280);
    toast('Sample playbook loaded');
  }

  function clearAll() {
    if (!st.nodes.length) return;
    st.nodes.forEach(n => { n.leaving = true; const el = nodeEls.get(n.id); if (el) el.style.cssText = nodeBoxStyle(n); });
    setTimeout(() => {
      st.nodes = []; st.edges = []; st.selected = null; st.parrot = 'greet';
      st.bubble = { show: true, kind: 'greet', text: "Fresh canvas! Drop a Supervisor to begin again — I'll keep you company.", options: [] };
      nodeEls.clear(); edgeEls.forEach(els => { els.path.remove(); els.dot.remove(); }); edgeEls.clear();
      nodesEl.innerHTML = '';
      render();
    }, 230);
  }

  function deleteNode(id) {
    const n = nodeById(id); if (!n) return;
    n.leaving = true; const el = nodeEls.get(id); if (el) el.style.cssText = nodeBoxStyle(n);
    setTimeout(() => {
      st.nodes = st.nodes.filter(x => x.id !== id);
      st.edges = st.edges.filter(e => e.from !== id && e.to !== id);
      if (st.selected === id) st.selected = null;
      nodeEls.delete(id); edgeEls.forEach((els, eid) => { if (eid.includes(id)) { els.path.remove(); els.dot.remove(); edgeEls.delete(eid); } });
      render();
    }, 220);
  }

  function validate() {
    const highTools = st.nodes.filter(n => n.type === 'tool' && n.risk === 'high');
    if (!highTools.length) {
      st.nodes.forEach(n => { n.invalid = false; n.warn = null; });
      setParrot('happy', 1700, true); toast('Looks good — no issues found');
      renderNodes(); renderInspector(); return;
    }
    const flagged = new Set();
    highTools.forEach(t => {
      const pe = st.edges.find(e => e.to === t.id);
      if (pe) flagged.add(pe.from);
    });
    st.nodes.forEach(n => {
      n.invalid = flagged.has(n.id);
      n.warn = n.invalid ? 'Needs approval' : null;
    });
    setParrot('concerned', 2800, true);
    st.bubble = { show: true, kind: 'greet', text: "A high-risk tool can act on its own. I'd add a human approval step before refunds go out — want me to set that up?", options: [] };
    toast('1 thing to review before saving', 'warn');
    renderNodes(); renderInspector(); renderPip();
  }

  // ── questionnaire ──
  function askQuestion() {
    const next = (st.qIndex + 1) % QUESTIONS.length;
    st.qIndex = next; st.parrotMin = false; st.selected = null;
    st.bubble = { show: true, kind: 'question', text: QUESTIONS[next].text, options: QUESTIONS[next].options };
    setParrot('curious'); renderPip(); renderNodes(); renderInspector();
  }

  function pickOption(label) {
    toast(`"${label}" noted`);
    setParrot('happy', 1500, true);
    const hasMore = st.qIndex < QUESTIONS.length - 1;
    if (hasMore) { setTimeout(askQuestion, 650); }
    else { setTimeout(() => { st.qIndex = -1; st.bubble = { show: true, kind: 'greet', text: "Perfect — that's everything I need. Your Billing playbook is ready to test.", options: [] }; renderPip(); }, 650); }
  }

  function dismissBubble() { st.bubble.show = false; st.qIndex = -1; renderPip(); }

  // ── toolbar wiring ──
  container.querySelector('.abc-add-sup')?.addEventListener('click',   e => { e.stopPropagation(); addSupervisor(); });
  container.querySelector('.abc-add-sub')?.addEventListener('click',   e => { e.stopPropagation(); addSubAgent(); });
  container.querySelector('.abc-add-tool')?.addEventListener('click',  e => { e.stopPropagation(); addTool(); });
  container.querySelector('.abc-seed-btn')?.addEventListener('click',  e => { e.stopPropagation(); seedSample(); });
  container.querySelector('.abc-clear-btn')?.addEventListener('click', e => { e.stopPropagation(); clearAll(); });
  container.querySelector('.abc-add-sup-empty')?.addEventListener('click', e => { e.stopPropagation(); addSupervisor(); });
  container.querySelector('.abc-seed-empty')?.addEventListener('click', e => { e.stopPropagation(); seedSample(); });
  container.querySelector('.abc-ask-btn')?.addEventListener('click',   e => { e.stopPropagation(); askQuestion(); });
  container.querySelector('.abc-pip-min')?.addEventListener('click',   e => { e.stopPropagation(); st.parrotMin = true; renderPip(); });
  pipPeek.addEventListener('click', () => { st.parrotMin = false; setParrot('idle'); });
  pipFig.addEventListener('click', e => { e.stopPropagation(); if (!st.transient) setParrot('happy', 1100, true); });

  // ── expose validate for Blazor ──
  container._abcValidate = validate;

  render();
  return { dispose() {
    cv.removeEventListener('pointerdown', onDown);
    cv.removeEventListener('pointermove', onMove);
    cv.removeEventListener('pointerup', onUp);
    cv.removeEventListener('pointerover', onOver);
    cv.removeEventListener('pointerout', onOut);
    window.removeEventListener('keydown', onKey);
    Object.values(timers).forEach(clearTimeout);
    container.innerHTML = '';
  }};
}

function esc(s) {
  return String(s ?? '').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}

// ── public API ──
export function init(containerId) {
  if (instances.has(containerId)) instances.get(containerId).dispose();
  const inst = createInstance(containerId);
  if (inst) instances.set(containerId, inst);
}

export function dispose(containerId) {
  if (instances.has(containerId)) { instances.get(containerId).dispose(); instances.delete(containerId); }
}

export function validate(containerId) {
  const el = document.getElementById(containerId);
  el?._abcValidate?.();
}
