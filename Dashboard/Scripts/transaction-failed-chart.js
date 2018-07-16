window.onload = function() {

    var address = "/dashboard/Home";
    //var address = "/Home";

    function trendingRibFailureChartData() {
        var failed = [];
        var failed21 = [];
        var failed98 = [];
        var failed99 = [];
        var failedNew = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/MonthlyRibFailureCummTrend",
            contentType: "application/json",
            datatype: 'json',
            async: false,
            success: function(data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    //if (i < 1) {
                    //    var k = i;
                    //} else {
                    //    k = i - 1;
                    //}
                    failed[i] = data[i]['FailureCount'];
                    //failedDiff[i] = data[i]['FailureCount'] - data[k]['FailureCount'];
                    failed21[i] = data[i]['Error21Count'];
                    failed98[i] = data[i]['Error98Count'];
                    failed99[i] = data[i]['Error99Count'];
                    failedNew[i] = data[i]['ErrorNewCount'];
                    dateTime[i] = data[i]['DateTime'];

                }
            }
        });

        var trendingChartData = {
            labels: dateTime, //dateTime,
            datasets: [
                {
                    hidden: false,
                    label: 'Total Failed',
                    backgroundColor: "rgba(20,20,20,0.8)",
                    borderColor: 'rgba(20,20,20,0.8)',
                    fill: false,
                    data: failed
                },
                {
                    hidden: false,
                    label: 'Error 21',
                    backgroundColor: "rgba(200,200,0,0.8)",
                    borderColor: 'rgba(200,200,0,0.8)',
                    fill: false,
                    data: failed21
                },
                {
                    hidden: false,
                    label: 'Error 98',
                    backgroundColor: "rgba(128,0,128,0.8)",
                    borderColor: 'rgba(128,0,128,0.8)',
                    fill: false,
                    data: failed98
                },
                {
                    hidden: false,
                    label: 'Error 99',
                    backgroundColor: "rgba(0,200,200,0.8)",
                    borderColor: 'rgba(0,200,200,0.8)',
                    fill: false,
                    data: failed99
                },
                {
                    hidden: false,
                    label: 'Error NEW',
                    backgroundColor: "rgba(200,2,2,0.8)",
                    borderColor: 'rgba(200,2,2,0.8)',
                    fill: false,
                    data: failedNew
                }
            ]

        };

        var ctx = document.getElementById("canvas_rib_failed").getContext("2d");
        window.myBar = new Chart(ctx, {
            type: 'bar',
            data: trendingChartData,
            options: {
                // Elements options apply to all of the options unless overridden in a dataset
                // In this case, we are setting the border of each bar to be 2px wide and green
                elements: {
                    rectangle: {
                        borderWidth: 2,
                        //borderColor: 'rgb(0, 0, 0)',
                        borderSkipped: 'bottom'
                    }
                },
                responsive: true,
                legend: {
                    position: 'bottom',
                },
                title: {
                    display: true,
                    text: 'RIB Monthly Failure'
                },
                scales: {
                    xAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Month'
                            }
                        }
                    ],
                    yAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Transaction Count'
                            },
                            ticks: {
                                suggestedMin: 0,
                                suggestedMax: 100,
                            }
                        }
                    ]
                }
            }
        });
    }


    trendingRibFailureChartData();
};