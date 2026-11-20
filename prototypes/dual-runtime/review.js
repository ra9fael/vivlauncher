"use strict";

const reviewText = {
  "zh-CN": {
    title: "图标与双版本外观", subtitle: "审核稿 03 / 保留现有布局，只调整标识与兼容控件。",
    try: "体验界面", states: "控件对照", svg: "打开 SVG 视觉稿", identity: "蓝底白标，保留官方箭头。",
    identityHint: "直接提取 AMD 官网 SVG 的两个多边形；方底使用主题蓝 #0067C0。", download: "下载图标 SVG",
    lightBackground: "浅色背景", darkBackground: "深色背景", actualSize: "实际像素尺寸 · 100% 缩放查看",
    preview: "交互预览", edition: "版本", modernNote: ".NET 10 / 保留官方 Fluent 的设计方向。切换版本仅改变控件外观，当前输入与选择会保留。",
    legacyNote: ".NET Framework 4.8 / 共用页面布局，使用兼容控件样式。功能与现代版一致；此处不切换真实运行时。",
    controlTitle: "同一套布局，两种控件外观", controlHint: "鼠标悬停、Tab 聚焦、展开与选择均可体验。这里只对照控件，不增加产品设置。",
    htmlNote: "HTML 表达设计方向；官方 Fluent 的实际绘制与系统文件选择器，以 WPF 实机验收为准。",
    sampleOpen: "打开工程", sampleDisabled: "不可用", sampleDirectory: "安装目录", sampleVersion: "打开方式",
    sampleMissing: "Vivado 2023.2 · 失效，不可选择", sampleWarning: "Vivado 2023.2 已失效，记录仍保留。",
    sampleNoLog: "不生成日志", sampleMore: "更多选项", sampleArguments: "附加参数", sampleReady: "演示控件，可自由操作。",
    sampleClicked: "已演示按钮响应，未创建真实进程。", modernSample: "Fluent 方向：柔和圆角，原生控件反馈。",
    legacySample: "兼容样式：简洁边框，清晰的键盘焦点。", light: "浅色", dark: "深色", missingBoth: "工程与版本同时失效", warningComparison: "三种失效状态对照"
  },
  en: {
    title: "Icon and two runtime editions", subtitle: "Review 03 / Existing layout, updated identity and compatible controls.",
    try: "Try the interface", states: "Compare controls", svg: "Open SVG review", identity: "The same arrow, in blue.",
    identityHint: "Two original polygons from AMD's official SVG, on our #0067C0 blue background.", download: "Download icon SVG",
    lightBackground: "Light background", darkBackground: "Dark background", actualSize: "Actual pixels · View at 100% zoom",
    preview: "Interactive preview", edition: "Edition", modernNote: ".NET 10 / Keeps the official Fluent direction. Switching editions preserves your input and selection.",
    legacyNote: ".NET Framework 4.8 / Shared layout with compatible controls. Same features; this preview does not switch real runtimes.",
    controlTitle: "Shared layout, two control styles", controlHint: "Try hovering, Tab focus, selection and expansion. These are review controls, not new product settings.",
    htmlNote: "HTML illustrates the design direction. Actual Fluent rendering and system file pickers will be verified in WPF.",
    sampleOpen: "Open project", sampleDisabled: "Unavailable", sampleDirectory: "Installation directory", sampleVersion: "Open with",
    sampleMissing: "Vivado 2023.2 · Unavailable", sampleWarning: "Vivado 2023.2 is unavailable. Its record is kept.",
    sampleNoLog: "No log file", sampleMore: "More options", sampleArguments: "Additional arguments", sampleReady: "Interactive demo controls.",
    sampleClicked: "Button response simulated. No real process created.", modernSample: "Fluent direction: softer corners and native feedback.",
    legacySample: "Compatible style: simple borders, visible keyboard focus.", light: "Light", dark: "Dark", missingBoth: "Project and version unavailable", warningComparison: "Compare unavailable states"
  }
};

for (const scheme of ["light", "dark"]) {
  for (const runtime of ["net10", "net48"]) {
    const sample = document.getElementById("control-template").content.firstElementChild.cloneNode(true);
    sample.dataset.scheme = scheme;
    sample.dataset.runtime = runtime;
    sample.classList.toggle("legacy-controls", runtime === "net48");
    sample.querySelector(".demo-action").addEventListener("click", () => {
      const status = sample.querySelector(".sample-status");
      status.dataset.review = "sampleClicked";
      status.textContent = reviewText[document.documentElement.lang].sampleClicked;
    });
    document.getElementById("control-grid").append(sample);
  }
}

function syncReview() {
  const messages = reviewText[document.documentElement.lang] || reviewText.en;
  document.querySelectorAll("[data-review]").forEach(node => { node.textContent = messages[node.dataset.review]; });
  document.querySelectorAll(".control-sample").forEach(sample => {
    const modern = sample.dataset.runtime === "net10";
    sample.querySelector(".sample-title").textContent = `${modern ? ".NET 10 · Fluent" : ".NET Framework 4.8"} / ${messages[sample.dataset.scheme]}`;
    sample.querySelector(".sample-description").textContent = messages[modern ? "modernSample" : "legacySample"];
  });
  document.querySelector('meta[name="color-scheme"]').content = document.documentElement.dataset.theme || "light dark";
  document.getElementById("runtime-note").textContent = messages[document.getElementById("runtime").value === "net48" ? "legacyNote" : "modernNote"];
  document.title = `${messages.title} · Vivado Launcher`;
}

document.getElementById("runtime").addEventListener("change", event => {
  const runtime = event.target.value;
  document.documentElement.dataset.runtime = runtime;
  document.querySelectorAll("#app, dialog").forEach(node => node.classList.toggle("legacy-controls", runtime === "net48"));
  syncReview();
});

new MutationObserver(syncReview).observe(document.documentElement, { attributes: true, attributeFilter: ["lang", "data-theme"] });
syncReview();
