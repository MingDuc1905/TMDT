(function($) {
    /* "use strict" */


 var dzChartlist = function(){
	
	var screenWidth = $(window).width();
		
	
	var activityBar = function(){
		// ═══ REPLACED: Fake Chart.js activity data removed — use real data from API ═══
	}
	var donutChart = function(){
		$("span.donut").peity("donut", {
			width: "140",
			height: "140",
			stroke: "#4d89f9",
			strokeWidth: "10",
		});
	}
	
	var chartBar = function(){
		// ═══ REPLACED: Fake ApexCharts data removed ═══
	}
	
	var counterBar = function(){
		$(".counter").counterUp({
			delay: 30,
			time: 3000
		});
	}
	
	
	/* Function ============ */
		return {
			init:function(){
			},
			
			
			load:function(){
				activityBar();		
				donutChart();	
				chartBar();
				counterBar();
			},
			
			resize:function(){
				
			}
		}
	
	}();

	jQuery(document).ready(function(){
	});
		
	jQuery(window).on('load',function(){
		setTimeout(function(){
			dzChartlist.load();
		}, 1000); 
		
	});

	jQuery(window).on('resize',function(){
		
		
	});     

})(jQuery);