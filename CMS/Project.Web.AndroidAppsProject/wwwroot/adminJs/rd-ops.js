/*!
 * Razdaneh Ops Console - rd-ops.js
 * افزونه‌های ظاهری تم رازدانه (بدون تداخل با admin.js / اسکریپت‌های داشبورد)
 */
(function ($) {
    'use strict';

    $(function () {
        /* سایه هدر هنگام اسکرول */
        var $header = $('.header-navbar');
        var onScroll = function () {
            var top = ($(document).scrollTop() || (window.pageYOffset || 0));
            $header.toggleClass('rd-scrolled', top > 8);
        };
        $(window).on('scroll.rdops', onScroll);
        onScroll();

        /* شمارش عدد KPI ها: <div class="rd-kpi-value" data-count="1240">0</div> */
        function countUp($el, target, dur) {
            var start = 0, t0 = null;
            function frame(ts) {
                if (!t0) t0 = ts;
                var p = Math.min((ts - t0) / dur, 1);
                p = 1 - Math.pow(1 - p, 3); // ease-out
                var val = Math.round(start + (target - start) * p);
                $el.text(val.toLocaleString('fa-IR'));
                if (p < 1) requestAnimationFrame(frame);
            }
            requestAnimationFrame(frame);
        }
        $('.rd-kpi-value[data-count]').each(function () {
            var $el = $(this);
            var target = parseInt($el.attr('data-count'), 10) || 0;
            countUp($el, target, 900);
        });

        /* درصدها در قالب ارقام فارسی */
        function faPct(n) {
            return (Math.round(n * 10) / 10).toLocaleString('fa-IR') + '٪';
        }

        /* دونات موفقیت/شکست:
           <div class="rd-donut" data-ok="9821" data-ko="179"></div> */
        $('.rd-donut[data-ok]').each(function () {
            var $d = $(this);
            var ok = parseInt($d.attr('data-ok'), 10) || 0;
            var ko = parseInt($d.attr('data-ko'), 10) || 0;
            var total = ok + ko || 1;
            var pctOk = ok / total * 100;
            var pctKo = 100 - pctOk;
            var r = 84, c = 2 * Math.PI * r;
            var okLen = c * pctOk / 100, koLen = c * pctKo / 100;
            $d.html(
                '<svg viewBox="0 0 200 200" width="200" height="200">' +
                '  <circle cx="100" cy="100" r="' + r + '" stroke="#f1f2fa"></circle>' +
                '  <circle cx="100" cy="100" r="' + r + '" stroke="#f0433d" stroke-dasharray="' + koLen + ' ' + c + '" stroke-dashoffset="' + (c - okLen) + '"></circle>' +
                '  <circle cx="100" cy="100" r="' + r + '" stroke="#14b874" stroke-dasharray="' + okLen + ' ' + c + '"></circle>' +
                '</svg>' +
                '<div class="rd-donut-center"><b>' + faPct(pctOk) + '</b><span>موفقیت کل</span></div>'
            );
        });

        /* نوارهای افقی: <div class="rd-hbar" data-value="98"></div> */
        $('.rd-hbar[data-value]').each(function () {
            var v = Math.max(0, Math.min(100, parseInt($(this).attr('data-value'), 10) || 0));
            var color = v >= 95 ? 'var(--rd-success)' : (v >= 80 ? 'var(--rd-warning)' : 'var(--rd-danger)');
            $(this).append('<div class="rd-hbar-fill" style="width:' + v + '%;background:' + color + '"></div>');
        });
    });

    /* نمایش/خاموشی افکت‌های ریز — در صورت نیاز: window.RDOps.pulse(...) */
    window.RDOps = {
        version: '1.0.0'
    };
})(jQuery);
