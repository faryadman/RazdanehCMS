/*!
 * Razdaneh Ops Console - rd-ops.js (v2)
 * افزونه‌های ظاهری تم رازدانه (بدون تداخل با admin.js / اسکریپت‌های داشبورد)
 */
(function ($) {
    'use strict';

    function faNum(n) {
        try { return Number(n).toLocaleString('fa-IR'); } catch (e) { return n; }
    }

    function countUp($el, target, dur) {
        if ($el.data('rd-counted')) return;
        $el.data('rd-counted', true);
        var t0 = null;
        function frame(ts) {
            if (!t0) t0 = ts;
            var p = Math.min((ts - t0) / dur, 1);
            p = 1 - Math.pow(1 - p, 3);
            $el.text(faNum(Math.round(target * p)));
            if (p < 1) requestAnimationFrame(frame);
        }
        requestAnimationFrame(frame);
    }

    function renderDonut($d) {
        if ($d.data('rd-donut-done')) return;
        var ok = parseInt($d.attr('data-ok'), 10) || 0;
        var ko = parseInt($d.attr('data-ko'), 10) || 0;
        var total = ok + ko;
        if (!total) return;
        $d.data('rd-donut-done', true);
        var pctOk = ok / total * 100;
        var r = 84, c = 2 * Math.PI * r;
        var okLen = c * pctOk / 100, koLen = c - okLen;
        $d.html(
            '<svg viewBox="0 0 200 200" width="200" height="200">' +
            '  <circle cx="100" cy="100" r="' + r + '" stroke="#f1f2fa"></circle>' +
            '  <circle cx="100" cy="100" r="' + r + '" stroke="#f0433d" stroke-dasharray="' + koLen + ' ' + c + '" stroke-dashoffset="' + (c - okLen) + '"></circle>' +
            '  <circle cx="100" cy="100" r="' + r + '" stroke="#14b874" stroke-dasharray="' + okLen + ' ' + c + '"></circle>' +
            '</svg>' +
            '<div class="rd-donut-center"><b>' + faNum(Math.round(pctOk * 10) / 10) + '٪</b><span>موفقیت کل</span></div>'
        );
    }

    function renderHbars($scope) {
        $scope.find('.rd-hbar[data-value]').each(function () {
            var $h = $(this);
            if ($h.data('rd-hbar-done')) return;
            var v = Math.max(0, Math.min(100, parseInt($h.attr('data-value'), 10) || 0));
            var color = v >= 95 ? 'var(--rd-success)' : (v >= 80 ? 'var(--rd-warning)' : 'var(--rd-danger)');
            $h.data('rd-hbar-done', true).append('<div class="rd-hbar-fill" style="width:' + v + '%;background:' + color + '"></div>');
        });
    }

    function rdInit(scope) {
        var $scope = $(scope || document);
        $scope.find('.rd-kpi-value[data-count]').each(function () {
            countUp($(this), parseInt($(this).attr('data-count'), 10) || 0, 900);
        });
        $scope.find('.rd-donut[data-ok]').each(function () { renderDonut($(this)); });
        renderHbars($scope);
    }

    $(function () {
        /* سایه هدر هنگام اسکرول */
        var $header = $('.header-navbar');
        var onScroll = function () {
            var top = ($(document).scrollTop() || (window.pageYOffset || 0));
            $header.toggleClass('rd-scrolled', top > 8);
        };
        $(window).on('scroll.rdops', onScroll);
        onScroll();

        rdInit(document);
    });

    /* عمومی‌سازی: برای محتوای داینامیک (مثلاً داشبورد) */
    window.RDOps = {
        version: '2.0.0',
        refresh: rdInit
    };
})(jQuery);
