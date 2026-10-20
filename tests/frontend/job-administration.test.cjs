const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const test = require('node:test');

const source = fs.readFileSync(path.join(__dirname, '../../CMS/Project.Web.AndroidAppsProject/wwwroot/adminJs/job.js'), 'utf8');

function environment(fetch, review = false, confirm = true) {
    let submit;
    let reloads = 0;
    const button = { disabled: false };
    const message = { hidden: true, textContent: '' };
    const form = {
        action: '/Admin/Job/Save', dataset: {},
        matches: selector => selector === '.job-form' || (review && selector === '.job-review'),
        querySelector: () => button,
    };
    const context = {
        document: { addEventListener: (_, handler) => { submit = handler; }, getElementById: () => message },
        window: { confirm: () => confirm, location: { reload: () => { reloads++; } } },
        FormData: class { constructor(target) { this.form = target; } },
        fetch,
    };
    vm.runInNewContext(source, context);
    return { form, button, message, reloads: () => reloads, submit: () => submit({ target: form, preventDefault() {} }) };
}

test('a successful form submits its token-bearing FormData and refreshes once', async () => {
    let request;
    const app = environment(async (url, options) => {
        request = { url, options };
        return { ok: true, json: async () => ({ status: '1' }) };
    });
    await app.submit();
    assert.equal(request.url, '/Admin/Job/Save');
    assert.equal(request.options.method, 'POST');
    assert.equal(request.options.credentials, 'same-origin');
    assert.equal(request.options.body.form, app.form);
    assert.equal(app.reloads(), 1);
});

test('a pending submission cannot create a second request', async () => {
    let calls = 0;
    let release;
    const pending = new Promise(resolve => { release = resolve; });
    const app = environment(async () => { calls++; await pending; return { ok: true, json: async () => ({ status: '1' }) }; });
    const first = app.submit();
    await app.submit();
    assert.equal(calls, 1);
    release();
    await first;
});

test('a network failure shows a Persian message and permits retry', async () => {
    const app = environment(async () => { throw new TypeError('Failed to fetch'); });
    await app.submit();
    assert.match(app.message.textContent, /ارتباط با سرور/);
    assert.equal(app.button.disabled, false);
    assert.equal(app.form.dataset.submitting, undefined);
    assert.equal(app.reloads(), 0);
});

test('server validation messages are displayed as text and preserve the form', async () => {
    const text = '<img src=x onerror=synthetic()> تنظیمات نامعتبر';
    const app = environment(async () => ({ ok: false, json: async () => ({ errors: [text] }) }));
    await app.submit();
    assert.equal(app.message.textContent, text);
    assert.equal(app.button.disabled, false);
    assert.equal(app.reloads(), 0);
});

test('an authentication redirect gives a sign-in message without parsing HTML as JSON', async () => {
    const app = environment(async () => ({ redirected: true, json: async () => { throw new Error('must not parse'); } }));
    await app.submit();
    assert.match(app.message.textContent, /حساب مدیر/);
    assert.equal(app.button.disabled, false);
});

test('review requires an explicit confirmation before sending the request', async () => {
    let calls = 0;
    const app = environment(async () => { calls++; }, true, false);
    await app.submit();
    assert.equal(calls, 0);
    assert.equal(app.button.disabled, false);
});
