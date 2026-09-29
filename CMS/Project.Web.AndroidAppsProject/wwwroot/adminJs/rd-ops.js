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

    /* ---------- دراپ‌داون منوی ردیف‌ها: قطعی، مستقل از data-api بوت‌استرپ ----------
       منو با position:fixed و محاسبهٔ JS قرار می‌گیرد؛ نه قلیپ توسط
       .table-responsive / .card می‌شود و نه از دید خارج می‌شود. */
    function rdMenuReset($menu) {
        $menu.removeClass('show').css({ position: '', top: '', left: '', right: '', visibility: '' }).removeData('rdTrigger');
    }
    function closeAllRdMenus(except) {
        $('.rd-item-menu.show').each(function () {
            if (except && this === except[0]) return;
            rdMenuReset($(this));
        });
        $('[data-rd-toggle="dropdown"]').attr('aria-expanded', 'false');
    }
    function rdMenuPosition($btn, $menu) {
        if (window.innerWidth < 768) return; // موبایل: منو درون سلول (static) باز می‌شود
        $menu.css({ position: 'fixed', top: 0, left: 0, right: 'auto', visibility: 'hidden' });
        var mw = $menu.outerWidth(), mh = $menu.outerHeight();
        var r = $btn[0].getBoundingClientRect();
        var vw = window.innerWidth, vh = window.innerHeight;
        var left = r.left;
        if (left + mw > vw - 8) left = Math.max(8, vw - mw - 8);
        if (left < 8) left = 8;
        var top = Math.max(8, r.bottom + 6); // همیشه زیر دکمه (داخل جدول) — فلیپ به بالا نه
        $menu.css({ top: top + 'px', left: left + 'px', visibility: '' });
    }
    $(document).on('click.rdops', '[data-rd-toggle="dropdown"]', function (e) {
        e.preventDefault();
        e.stopPropagation();
        var $btn = $(this);
        var $menu = $btn.siblings('.dropdown-menu').first();
        if (!$menu.length) return;
        var wasOpen = $menu.hasClass('show');
        closeAllRdMenus($menu);
        if (wasOpen) return;
        $menu.data('rdTrigger', $btn).addClass('show');
        $btn.attr('aria-expanded', 'true');
        rdMenuPosition($btn, $menu);
    });
    $(document).on('click.rdops', '.rd-item-menu .dropdown-item', function () {
        setTimeout(function () { closeAllRdMenus(); }, 0);
    });
    $(document).on('click.rdops', function (e) {
        if (!$(e.target).closest('.rd-item-dropdown').length) closeAllRdMenus();
    });
    $(document).on('keyup.rdops', function (e) {
        if (e.key === 'Escape') closeAllRdMenus();
    });
    /* اسکرول (هر کانتینر، حتی داخل جدول) و رزایز → منو دنبال دکمه بماند، نه اینکه بسته شود */
    function rdReposition() {
        $('.rd-item-menu.show').each(function () {
            var $m = $(this), $b = $m.data('rdTrigger');
            if (!$b || !$b.length) return;
            var rect = $b[0].getBoundingClientRect();
            if (rect.bottom < 0 || rect.top > window.innerHeight) { rdMenuReset($m); return; }
            rdMenuPosition($b, $m);
        });
    }
    document.addEventListener('scroll', rdReposition, true);
    $(window).on('resize.rdops', rdReposition);

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
