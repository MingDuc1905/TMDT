(function($) {
    /* "use strict" */
    // ═══ REPLACED: Remove fake ApexCharts data — use real Chart.js from Dashboard.cshtml ═══
    // The Dashboard.cshtml now uses inline Chart.js with real DB data from AdminController API.
    // This file is kept as a no-op to avoid 404 errors from the layout include.

    var dzChartlist = function() {
        return {
            init: function() { },
            load: function() { },
            resize: function() { }
        };
    }();

    jQuery(document).ready(function() { });
    jQuery(window).on('load', function() {
        setTimeout(function() { dzChartlist.load(); }, 1000);
    });
    jQuery(window).on('resize', function() { });
})(jQuery);
