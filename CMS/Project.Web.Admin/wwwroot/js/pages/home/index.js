"use strict";

var chart;
var pieChart;
var KTDashboard = function () {

    // Define fonts
    var fontFamily = KTUtil.getCssVariableValue('--bs-font-sans-serif');

    // Chart labels
    const labels = ['آبان', 'آذر', 'دی', 'بهمن', 'اسفند'];

    var initStatsBarChart = function (chartData) {
        var element = document.getElementById('kt_apexcharts_1');
        var height = parseInt(KTUtil.css(element, 'height'));
        var labelColor = KTUtil.getCssVariableValue('--bs-gray-500');
        var borderColor = KTUtil.getCssVariableValue('--bs-gray-200');
        var baseColor = KTUtil.getCssVariableValue('--bs-success');
        var secondaryColor = KTUtil.getCssVariableValue('--bs-primary');

        if (!element) {
            return;
        }

        var options = {
            noData: {
                text: 'درحال دریافت اطلاعات...',
                align: 'center',
                verticalAlign: 'middle',
                offsetX: 0,
                offsetY: 0,
                style: {
                    fontSize: '14px'
                }
            },
            series: [
                {
                    name: 'سود',
                    data: chartData[0].columnValue
                },
                {
                    name: 'بارمان',
                    data: chartData[1].columnValue
                },
            ],
            chart: {
                fontFamily: 'inherit',
                type: 'bar',
                height: height,
                toolbar: {
                    show: true
                },
            },
            plotOptions: {
                bar: {
                    horizontal: false,
                    columnWidth: ['80%'],
                },
            },
            legend: {
                show: true
            },
            dataLabels: {
                enabled: false
            },
            stroke: {
                show: true,
                width: 2,
                colors: ['transparent']
            },
            xaxis: {
                categories: chartData[0].columnName,
                axisBorder: {
                    show: true,
                },
                axisTicks: {
                    show: true
                },
                labels: {
                    style: {
                        colors: labelColor,
                        fontSize: '12px'
                    }
                }
            },
            yaxis: {
                labels: {
                    style: {
                        colors: labelColor,
                        fontSize: '12px'
                    }
                }
            },
            fill: {
                opacity: 1
            },
            states: {
                normal: {
                    filter: {
                        type: 'none',
                        value: 0
                    }
                },
                hover: {
                    filter: {
                        type: 'none',
                        value: 0
                    }
                },
                active: {
                    allowMultipleDataPointsSelection: false,
                    filter: {
                        type: 'none',
                        value: 0
                    }
                }
            },
            tooltip: {
                style: {
                    fontSize: '12px'
                },
                y: {
                    formatter: function (val) {
                        return val + ' تومان'
                    }
                }
            },
            colors: [baseColor, secondaryColor],
            grid: {
                borderColor: borderColor,
                strokeDashArray: 4,
                yaxis: {
                    lines: {
                        show: true
                    }
                }
            }
        };

        chart = new ApexCharts(element, options);

        chart.render();
    }

    var initStatsPieChart = function () {
        var ctx = document.getElementById('kt_chartjs_3');
        const config = {
            type: 'pie',
            data: {},
            options: {
                plugins: {
                    title: {
                        display: false,
                    }
                },
                responsive: true,
            },
            defaults: {
                global: {
                    defaultFont: fontFamily
                }
            }
        };
        pieChart = new Chart(ctx, config);
    }

    return {
        init: function () {
            initStatsPieChart();
            //initStatsBarChart();


            //TransactionsChartPerCityData
            fetch(`/transactionspiechartdata`,
                {
                    method: 'GET',
                })
                .then(response => response.json())
                .then(result => {
                    let backgroundColors = [];
                    for (var i = 0; i < result.categories.length; i++) {
                        backgroundColors.push(colors[i]);
                    }
                    pieChart.data = {
                        labels: result.categories,
                        datasets: [
                            {
                                label: 'Dataset 1',
                                data: result.amounts,
                                backgroundColor: backgroundColors
                            },
                        ]
                    }
                    pieChart.update();
                })
                .catch(error => { console.log(error) });

            fetch(`/transactionschartdata`,
                {
                    method: 'GET',
                })
                .then(response => response.json())
                .then(result => {
                    initStatsBarChart(result);
                    console.log(result);
                })
                .catch(error => { console.log(error) });

            //setTimeout(() => {
            //    chart.updateSeries([
            //        {
            //            name: 'سود ماهانه',
            //            data: [4400, 5500, 5700, 5600, 6100, 5800]
            //        },
            //        {
            //            name: 'سود بارمان',
            //            data: [440, 500, 700, 560, 1000, 800]
            //        }
            //    ]);
            //}, 1234);
        }
    }
}();

KTUtil.onDOMContentLoaded(function () {
    KTDashboard.init();
});

