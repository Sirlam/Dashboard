//window.onload = function () {

    var address = "/dashboard/OnBoarding";
    //var address = "/OnBoarding";

    function trendingCardIssuedChartData() {
        var count = [];
        var cs = [];
        var cm = [];
        var others = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/CardOnboardingTrend",
            contentType: "application/json",
            datatype: 'json',
            async: false,
            success: function (data) {
                console.log(data);
                for (var i = 0; i < data.length; i++) {
                    count[i] = data[i]['MerchantCount'];
                    cs[i] = data[i]['Cs'];
                    cm[i] = data[i]['Cm'];
                    others[i] = data[i]['Unknown'];
                    dateTime[i] = data[i]['DateApproved'];
                    //console.log('rib ' + rib);
                }
            }
        });

        var trendingChartData = {
            labels: dateTime, //dateTime,
            datasets: [
                {
                    label: 'Card Issued Count',
                    backgroundColor: "rgba(20,20,20,0.8)",
                    borderColor: 'rgba(20,20,20,0.8)',
                    fill: false,
                    data: count
                }, {
                    label: 'CS',
                    backgroundColor: "rgba(200,200,0,0.8)",
                    borderColor: 'rgba(200,200,0,0.8)',
                    fill: false,
                    data: cs
                }, {
                    label: 'CM',
                    backgroundColor: "rgba(128,0,128,0.8)",
                    borderColor: 'rgba(128,0,128,0.8)',
                    fill: false,
                    data: cm
                }, { label: 'Others',
                    backgroundColor: "rgba(200,2,2,0.8)",
                    borderColor: 'rgba(200,2,2,0.8)',
                    fill: false,
                    data: others
                }
            ]

        };

        var ctx = document.getElementById("card_onboarding").getContext("2d");
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
                    text: 'Card Issued Chart'
                },
                scales: {
                    xAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Date Issued'
                            }
                        }
                    ],
                    yAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Card Issued'
                            }//,
                            //ticks: {
                            //    suggestedMin: 0,
                            //    suggestedMax: 100,
                            //}
                        }
                    ]
                }
            }
        });
    }
function trendingRibChartData() {
        var count = [];
        var cs = [];
        var cm = [];
        var db = [];
        var others = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/RibOnboardingTrend",
            contentType: "application/json",
            datatype: 'json',
            async: false,
            success: function (data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    count[i] = data[i]['MerchantCount'];
                    dateTime[i] = data[i]['DateApproved'];
                    //console.log('rib ' + rib);
                }
            }
        });

        var trendingChartData = {
            labels: dateTime, //dateTime,
            datasets: [
                {
                    label: 'Rib Onboarded Count',
                    backgroundColor: "rgba(20,20,20,0.8)",
                    borderColor: 'rgba(20,20,20,0.8)',
                    fill: false,
                    data: count
                }
            ]

        };

        var ctx = document.getElementById("rib_onboarding").getContext("2d");
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
                    text: 'RIB Onboarded Chart'
                },
                scales: {
                    xAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Date Created'
                            }
                        }
                    ],
                    yAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'RIB Onboarded'
                            }//,
                            //ticks: {
                            //    suggestedMin: 0,
                            //    suggestedMax: 100,
                            //}
                        }
                    ]
                }
            }
        });
}

function trendingCasaChartData() {
    var count = [];
    var savings = [];
    var current = [];
    var others = [];
    var dateTime = [];
    $.ajax({
        type: "GET",
        url: address + "/CasaDepositsTrend",
        contentType: "application/json",
        datatype: 'json',
        async: false,
        success: function (data) {
            //console.log(data);
            for (var i = 0; i < data.length; i++) {
                count[i] = data[i]['Count'];
                savings[i] = data[i]['SavingsCount'];
                current[i] = data[i]['CurrentCount'];
                others[i] = data[i]['OthersCount'];
                dateTime[i] = data[i]['TransactionDate'];
                //console.log('rib ' + rib);
            }
        }
    });

    var trendingChartData = {
        labels: dateTime, //dateTime,
        datasets: [
            {
                label: 'Total',
                backgroundColor: "rgba(20,20,20,0.8)",
                borderColor: 'rgba(20,20,20,0.8)',
                fill: false,
                data: count
            }, {
                label: 'Savings',
                backgroundColor: "rgba(200,200,0,0.8)",
                borderColor: 'rgba(200,200,0,0.8)',
                fill: false,
                data: savings
            }, {
                label: 'Current',
                backgroundColor: "rgba(128,0,128,0.8)",
                borderColor: 'rgba(128,0,128,0.8)',
                fill: false,
                data: current
            }, {
                label: 'Others',
                backgroundColor: "rgba(200,2,2,0.8)",
                borderColor: 'rgba(200,2,2,0.8)',
                fill: false,
                data: others
            }
        ]

    };

    var ctx = document.getElementById("casa_deposits").getContext("2d");
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
                text: 'CASA Deposits Chart'
            },
            scales: {
                xAxes: [
                    {
                        display: true,
                        scaleLabel: {
                            display: true,
                            labelString: 'Transactions Date'
                        }
                    }
                ],
                yAxes: [
                    {
                        display: true,
                        scaleLabel: {
                            display: true,
                            labelString: 'Deposits Count'
                        }//,
                        //ticks: {
                        //    suggestedMin: 0,
                        //    suggestedMax: 100,
                        //}
                    }
                ]
            }
        }
    });
}


console.log("Running");
    trendingRibChartData();
    trendingCardIssuedChartData();
    trendingCasaChartData();

//};