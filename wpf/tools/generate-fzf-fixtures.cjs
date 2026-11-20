const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const root = path.resolve(__dirname, '..', '..');
const bundlePath = path.join(root, 'prototypes/wpf-launcher/vendor/fzf-0.5.2.umd.js');
const fzf = require(bundlePath);
const bundle = fs.readFileSync(bundlePath, 'utf8');
const normalizationCode = bundle.slice(bundle.indexOf('const z={'), bundle.indexOf('function E(t)'));
const normalization = vm.runInNewContext(normalizationCode + '; z');
fs.writeFileSync(path.join(root, 'wpf/VivLauncher.Core/FzfNormalization.json'), JSON.stringify(normalization, null, 2) + '\n');
const prototype = fs.readFileSync(path.join(root, 'prototypes/wpf-launcher/app.js'), 'utf8');
const projects = vm.runInNewContext(prototype.match(/const baseProjects = (\[[\s\S]*?\n\]);/)[1]);
const texts = [...projects.map(project => project.path), 'D:\\设计\\工程一\\信号处理.xpr', 'C:\\FPGA projects\\café_École.xpr', 'fooBar123_Baz', ' hello world ', 'D:\\😀\\设计.xpr'];
const queries = ['', 'pjt1', 'zu15 xpr', 'base', 'zu15 !pynq', "'pjt1", "'pynq | 'hifi", 'PYNQ', '设计 工一', '^D:', '.xpr$', '!^C:', '^hello world$', 'hello\\ world', 'fB', 'cafe', 'école', "!'pjt1", '^fooBar123_Baz$', '😀', 'x | !foo'];
const fixtures = [];
function add(text, query) {
  delete require.cache[require.resolve(bundlePath)];
  const fresh = require(bundlePath);
  const found = new fresh.Fzf([text], { fuzzy: 'v2', match: fresh.extendedMatch, casing: 'smart-case', tiebreakers: [] }).find(query)[0];
  fixtures.push({ text, query, score: found?.score ?? null, positions: found ? [...found.positions].sort((first, second) => first - second) : [] });
}
for (const text of texts) for (const query of queries) add(text, query);
for (const [code, normalized] of Object.entries(normalization)) add(String.fromCodePoint(+code), normalized);
let seed = 42017;
function random(max) { seed = (Math.imul(seed, 1664525) + 1013904223) >>> 0; return seed % max; }
const alphabet = 'aaabbcDD_12-/. 中文éÉ';
for (let iteration = 0; iteration < 500; iteration++) {
  const text = Array.from({length: 8 + random(80)}, () => alphabet[random(alphabet.length)]).join('');
  const query = Array.from({length: 1 + random(6)}, () => alphabet[random(alphabet.length)]).join('');
  add(text, query);
}
add('abc'.repeat(600), 'abc'.repeat(40));
fs.writeFileSync(path.join(root, 'wpf/VivLauncher.Tests/fzf-fixtures.json'), JSON.stringify(fixtures) + '\n');
console.log(`Generated ${fixtures.length} fzf parity fixtures and ${Object.keys(normalization).length} normalization mappings.`);
