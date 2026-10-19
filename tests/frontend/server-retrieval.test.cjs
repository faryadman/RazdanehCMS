const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

function loadRenderer() {
    const rows = [];
    const values = new Map();
    const handlers = new Map();
    const requests = [];
    const alerts = [];
    const elements = new Map();
    let drawCount = 0;
    const table = {
        row: { add(cells) { const row = { cells }; rows.push(row); return { node: () => row }; } },
        rows: { add(row) { rows.push(row); return table; } },
        draw() { drawCount++; return table; },
        clear() { rows.length = 0; return table; },
        on() { return table; }, off() { return table; },
        page() { return table; }
    };
    function $(selector) {
        return {
            DataTable: () => table,
            on(event, child, handler) { if (handler) handlers.set(`${selector}:${event}:${child}`, handler); return this; },
            change() { return this; }, off() { return this; }, css() { return this; }, modal() { return this; },
            attr(name, value) { if (typeof selector === 'object') return selector.attributes?.[name]; return value; },
            addClass() { return this; },
            val(value) { if (arguments.length) { values.set(selector, value); return this; } return values.get(selector) ?? ''; }
        };
    }
    $.ajax = options => { requests.push(options); };
    const context = vm.createContext({
        $, console: { log() {} }, localStorage: { getItem: () => null, setItem() {} },
        window: { location: { pathname: '/admin/servers' } },
        document: { getElementById(id) { if (!elements.has(id)) elements.set(id, {}); return elements.get(id); } },
        loading() {}, Swal: { close() {} }, swal: { close() {} }, toastr: { error() {} },
        setInterval: () => 0, clearInterval() {},
        alert: message => alerts.push(message)
    });
    vm.runInContext(fs.readFileSync(path.join(__dirname,
        '../../CMS/Project.Web.AndroidAppsProject/wwwroot/adminJs/server.js'), 'utf8'), context);
    return { context, rows, values, handlers, requests, alerts, drawCount: () => drawCount };
}

function server(overrides = {}) {
    return {
        id: 42, serverName: 'نمونه', location: 'نمونه', ip: '192.0.2.10', currentDomainValue: 'example.invalid',
        updatedAtFormatted: 'تاریخ نمونه', group: { title: 'گروه نمونه' }, isAvailable: true,
        config: JSON.stringify({ outbounds: [{ streamSettings: {
            tlsSettings: { serverName: 'sample.example.invalid' }, wsSettings: { headers: { Host: 'sample.example.invalid' } }
        } }] }), ...overrides
    };
}

test('a malformed configuration does not hide the remaining servers', () => {
    const app = loadRenderer();
    app.context.renderservers([server({ config: '{' }), server({ id: 43 })]);
    assert.equal(app.rows.length, 2);
    assert.match(app.rows[0].cells[4], /تنظیمات JSON نامعتبر است/);
    assert.match(app.rows[1].cells[4], /sample\.example\.invalid/);
});

test('missing optional JSON paths and a missing group do not stop rendering', () => {
    const app = loadRenderer();
    app.context.renderservers([server({ config: '{}', group: null })]);
    assert.equal(app.rows.length, 1);
    assert.match(app.rows[0].cells[7], /—/);
});

test('the renderer inserts each server once and flushes queued rows in one draw', () => {
    const app = loadRenderer();
    app.context.renderservers([server(), server({ id: 43 })]);
    assert.equal(app.rows.length, 2);
    assert.equal(app.drawCount(), 1);
});

test('server and configuration text is escaped before it becomes table HTML', () => {
    const app = loadRenderer();
    const untrusted = '<img src=x onerror="example()">';
    const config = JSON.stringify({ outbounds: [{ streamSettings: { tlsSettings: { serverName: untrusted } } }] });
    app.context.renderservers([server({ serverName: untrusted, config, group: { title: untrusted }, location: untrusted,
        currentDomainValue: untrusted, ip: untrusted })]);
    const html = app.rows[0].cells.join('');
    assert.doesNotMatch(html, /<img/);
    assert.match(html, /&lt;img/);
});

test('the edit form uses the freshly fetched details instead of stale table attributes', () => {
    const app = loadRenderer();
    const button = { attributes: { 'data-item-id': '42', 'data-item-serverName': 'stale',
        'data-item-location': 'stale', 'data-item-ip': '192.0.2.99' } };
    app.handlers.get('#serversTable:click:.edit').call(button);
    const request = app.requests.find(item => item.url.includes('/Detail?id=42'));
    assert.ok(request);
    request.success(server({ serverName: 'نام تازه', location: 'محل تازه', ip: '192.0.2.20' }));
    assert.equal(app.values.get('#ServerName'), 'نام تازه');
    assert.equal(app.values.get('#Location'), 'محل تازه');
    assert.equal(app.values.get('#Ip'), '192.0.2.20');
});

test('a failed read shows a fetch error instead of assuming an internet failure', () => {
    const app = loadRenderer();
    app.context.getservers(false, 0);
    app.requests[0].error({ status: 500 });
    assert.match(app.alerts[0], /واکشی اطلاعات سرورها/);
});

test('an unexpected response shape does not erase the previously displayed servers', () => {
    const app = loadRenderer();
    app.context.renderservers([server()]);
    app.context.getservers(false, 0);
    app.requests[0].success({ message: 'پاسخ نامعتبر' });
    assert.equal(app.rows.length, 1);
    assert.match(app.alerts[0], /پاسخ واکشی اطلاعات سرورها معتبر نیست/);
});
