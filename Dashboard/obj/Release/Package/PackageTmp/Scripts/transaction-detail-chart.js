window.onload = function () {

    var address = "/dashboard/Home";
    //var address = "/Home";
    var dateStr = document.getElementById("InputDate").value;

    function trendingMobileChartData(dateStr) {
        var success = [];
        var reversed = [];
        var pending = [];
        var failed = [];
        var failedDiff = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/CummMobileTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { detailDate: dateStr },
            async: false,
            success: function (data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    if (i < 1) {
                        var k = i;
                    } else {
                        k = i - 1;
                    }
                    success[i] = data[i]['SuccessCount'];
                    reversed[i] = data[i]['ReversedCount'];
                    pending[i] = data[i]['PendingCount'];
                    failed[i] = data[i]['FailureCount'];
                    failedDiff[i] = data[i]['FailureCount'] - data[k]['FailureCount'];
                    dateTime[i] = data[i]['DateTime'];
                    //console.log('rib ' + rib);
                }
            }
        });

        var trendingChartData = {
            labels: dateTime, //dateTime,
            datasets: [
                {
                    label: 'Success',
                    backgroundColor: "rgba(20,180,120,0.8)",
                    borderColor: 'rgba(20,180,120,0.8)',
                    fill: false,
                    data: success
                }, {
                    label: 'Reversal',
                    hidden: true,
                    backgroundColor: "rgba(226,133,42,0.8)",
                    borderColor: 'rgba(226,133,42,0.8)',
                    fill: false,
                    data: reversed
                }, {
                    label: 'Pending',
                    hidden: true,
                    backgroundColor: "rgba(226,133,42,0.8)",
                    borderColor: 'rgba(226,133,42,0.8)',
                    fill: false,
                    data: pending
                }, {
                    label: 'Failed',
                    hidden: false,
                    backgroundColor: "rgb(255,0,0)",
                    borderColor: 'rgb(255,0,0)',
                    fill: false,
                    data: failed
                }, {
                    label: 'Failed Difference',
                    hidden: false,
                    backgroundColor: "rgb(200,180,120)",
                    borderColor: 'rgb(200,180,120)',
                    fill: false,
                    data: failedDiff
                }

            ]

        };

        var ctx = document.getElementById("canvas_mobile").getContext("2d");
        window.myBar = new Chart(ctx, {
            type: 'line',
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
                    text: 'Mobile Transactions Chart'
                },
                scales: {
                    xAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Time'
                            }
                        }
                    ],
                    yAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Success Percentage'
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

    function trendingTopupChartData(dateStr) {
        var success = [];
        var reversed = [];
        var pending = [];
        var failed = [];
        var failedDiff = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/CummTopUpTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { detailDate: dateStr },
            async: false,
            success: function (data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    if (i < 1) {
                        var k = i;
                    } else {
                        k = i - 1;
                    }
                    success[i] = data[i]['SuccessCount'];
                    reversed[i] = data[i]['ReversedCount'];
                    pending[i] = data[i]['PendingCount'];
                    failed[i] = data[i]['FailureCount'];
                    failedDiff[i] = data[i]['FailureCount'] - data[k]['FailureCount'];
                    dateTime[i] = data[i]['DateTime'];
                    //console.log('rib ' + rib);
                }
            }
        });

        var trendingChartData = {
            labels: dateTime, //dateTime,
            datasets: [
                {
                    label: 'Success',
                    backgroundColor: "rgba(20,180,120,0.8)",
                    borderColor: 'rgba(20,180,120,0.8)',
                    fill: false,
                    data: success
                }, {
                    label: 'Reversal',
                    hidden: true,
                    backgroundColor: "rgba(226,133,42,0.8)",
                    borderColor: 'rgba(226,133,42,0.8)',
                    fill: false,
                    data: reversed
                }, {
                    label: 'Pending',
                    hidden: true,
                    backgroundColor: "rgba(226,133,42,0.8)",
                    borderColor: 'rgba(226,133,42,0.8)',
                    fill: false,
                    data: pending
                }, {
                    label: 'Failed',
                    hidden: false,
                    backgroundColor: "rgb(255,0,0)",
                    borderColor: 'rgb(255,0,0)',
                    fill: false,
                    data: failed
                }, {
                    label: 'Failed Difference',
                    hidden: false,
                    backgroundColor: "rgb(200,180,120)",
                    borderColor: 'rgb(200,180,120)',
                    fill: false,
                    data: failedDiff
                }

            ]

        };

        var ctx = document.getElementById("canvas_topup").getContext("2d");
        window.myBar = new Chart(ctx, {
            type: 'line',
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
                    text: 'Topup Chart'
                },
                scales: {
                    xAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Time'
                            }
                        }
                    ],
                    yAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Success Percentage'
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

    function trendingInterBankChartData(dateStr) {
        var success = [];
        var reversed = [];
        var pending = [];
        var failed = [];
        var failedDiff = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/CummInterBankTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { detailDate: dateStr },
            async: false,
            success: function (data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    if (i < 1) {
                        var k = i;
                    } else {
                        k = i - 1;
                    }
                    success[i] = data[i]['SuccessCount'];
                    reversed[i] = data[i]['ReversedCount'];
                    pending[i] = data[i]['PendingCount'];
                    failed[i] = data[i]['FailureCount'];
                    failedDiff[i] = data[i]['FailureCount'] - data[k]['FailureCount'];
                    dateTime[i] = data[i]['DateTime'];
                    //console.log('rib ' + rib);
                }
            }
        });

        var trendingChartData = {
            labels: dateTime, //dateTime,
            datasets: [
                {
                    label: 'Success',
                    backgroundColor: "rgba(20,180,120,0.8)",
                    borderColor: 'rgba(20,180,120,0.8)',
                    fill: false,
                    data: success
                }, {
                    label: 'Reversal',
                    hidden: true,
                    backgroundColor: "rgba(226,133,42,0.8)",
                    borderColor: 'rgba(226,133,42,0.8)',
                    fill: false,
                    data: reversed
                }, {
                    label: 'Pending',
                    hidden: true,
                    backgroundColor: "rgba(226,133,42,0.8)",
                    borderColor: 'rgba(226,133,42,0.8)',
                    fill: false,
                    data: pending
                }, {
                    label: 'Failed',
                    hidden: false,
                    backgroundColor: "rgb(255,0,0)",
                    borderColor: 'rgb(255,0,0)',
                    fill: false,
                    data: failed
                }, {
                    label: 'Failed Difference',
                    hidden: false,
                    backgroundColor: "rgb(200,180,120)",
                    borderColor: 'rgb(200,180,120)',
                    fill: false,
                    data: failedDiff
                }

            ]

        };

        var ctx = document.getElementById("canvas_interbank").getContext("2d");
        window.myBar = new Chart(ctx, {
            type: 'line',
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
                    text: 'InterBank Transfers Chart'
                },
                scales: {
                    xAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Time'
                            }
                        }
                    ],
                    yAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Success Percentage'
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

    function trendingXpressBoardingChartData(dateStr) {
        var success = [];
        var reversed = [];
        var pending = [];
        var failed = [];
        var failedDiff = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/CummXpressBoardingTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { detailDate: dateStr },
            async: false,
            success: function (data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    if (i < 1) {
                        var k = i;
                    } else {
                        k = i - 1;
                    }
                    success[i] = data[i]['SuccessCount'];
                    reversed[i] = data[i]['ReversedCount'];
                    pending[i] = data[i]['PendingCount'];
                    failed[i] = data[i]['FailureCount'];
                    failedDiff[i] = data[i]['FailureCount'] - data[k]['FailureCount'];
                    dateTime[i] = data[i]['DateTime'];
                    //console.log('rib ' + rib);
                }
            }
        });

        var trendingChartData = {
            labels: dateTime, //dateTime,
            datasets: [
                {
                    label: 'Success',
                    backgroundColor: "rgba(20,180,120,0.8)",
                    borderColor: 'rgba(20,180,120,0.8)',
                    fill: false,
                    data: success
                }, {
                    label: 'Reversal',
                    hidden: true,
                    backgroundColor: "rgba(226,133,42,0.8)",
                    borderColor: 'rgba(226,133,42,0.8)',
                    fill: false,
                    data: reversed
                }, {
                    label: 'Pending',
                    hidden: true,
                    backgroundColor: "rgba(226,133,42,0.8)",
                    borderColor: 'rgba(226,133,42,0.8)',
                    fill: false,
                    data: pending
                }, {
                    label: 'Failed',
                    hidden: false,
                    backgroundColor: "rgb(255,0,0)",
                    borderColor: 'rgb(255,0,0)',
                    fill: false,
                    data: failed
                }, {
                    label: 'Failed Difference',
                    hidden: false,
                    backgroundColor: "rgb(200,180,120)",
                    borderColor: 'rgb(200,180,120)',
                    fill: false,
                    data: failedDiff
                }

            ]

        };

        var ctx = document.getElementById("canvas_xpress_boarding").getContext("2d");
        window.myBar = new Chart(ctx, {
            type: 'line',
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
                    text: 'Xpress Account Onboarding Chart'
                },
                scales: {
                    xAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Time'
                            }
                        }
                    ],
                    yAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Success Percentage'
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

    function trendingRibChartData(dateStr) {
        var success = [];
        var reversed = [];
        var pending = [];
        var failed = [];
        var failedDiff = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/CummRibTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { detailDate: dateStr },
            async: false,
            success: function (data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    if (i < 1) {
                        var k = i;
                    } else {
                        k = i - 1;
                    }
                    success[i] = data[i]['SuccessCount'];
                    reversed[i] = data[i]['ReversedCount'];
                    pending[i] = data[i]['PendingCount'];
                    failed[i] = data[i]['FailureCount'];
                    failedDiff[i] = data[i]['FailureCount'] - data[k]['FailureCount'];
                    dateTime[i] = data[i]['DateTime'];
                    //console.log('rib ' + rib);
                }
            }
        });

        var trendingChartData = {
            labels: dateTime, //dateTime,
            datasets: [
                {
                    label: 'Success',
                    backgroundColor: "rgba(20,180,120,0.8)",
                    borderColor: 'rgba(20,180,120,0.8)',
                    fill: false,
                    data: success
                }, {
                    label: 'Reversal',
                    hidden: true,
                    backgroundColor: "rgba(226,133,42,0.8)",
                    borderColor: 'rgba(226,133,42,0.8)',
                    fill: false,
                    data: reversed
                }, {
                    label: 'Pending',
                    hidden: true,
                    backgroundColor: "rgba(226,133,42,0.8)",
                    borderColor: 'rgba(226,133,42,0.8)',
                    fill: false,
                    data: pending
                }, {
                    label: 'Failed',
                    hidden: false,
                    backgroundColor: "rgb(255,0,0)",
                    borderColor: 'rgb(255,0,0)',
                    fill: false,
                    data: failed
                }, {
                    label: 'Failed Difference',
                    hidden: false,
                    backgroundColor: "rgb(200,180,120)",
                    borderColor: 'rgb(200,180,120)',
                    fill: false,
                    data: failedDiff
                }

            ]

        };

        var ctx = document.getElementById("canvas_rib").getContext("2d");
        window.myBar = new Chart(ctx, {
            type: 'line',
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
                    text: 'RIB Transactions Chart'
                },
                scales: {
                    xAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Time'
                            }
                        }
                    ],
                    yAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Success Percentage'
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

    function trendingNipOutChartData(dateStr) {
        var success = [];
        var reversal = [];
        var incomplete = [];
        var failed = [];
        var failedDiff = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/CummNipOutgoingTrend",
            contentType: "application/json",
            datatype: 'json',
            async: false,
            data: { detailDate: dateStr },
            success: function (data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    if (i < 1) {
                        var k = i;
                    } else {
                        k = i - 1;
                    }
                    success[i] = data[i]['SuccessCount'];
                    reversal[i] = data[i]['ReversedCount'];
                    incomplete[i] = data[i]['IncompleteCount'];
                    failed[i] = data[i]['FailureCount'];
                    failedDiff[i] = data[i]['FailureCount'] - data[k]['FailureCount'];
                    dateTime[i] = data[i]['DateTime'];
                    //console.log('rib ' + rib);
                }
            }
        });

        var trendingChartData = {
            labels: dateTime, //dateTime,
            datasets: [
               {
                   label: 'Success',
                   backgroundColor: "rgba(20,180,120,0.8)",
                   borderColor: 'rgba(20,180,120,0.8)',
                   fill: false,
                   data: success
               }, {
                   label: 'Reversal',
                   hidden: true,
                   backgroundColor: "rgba(226,133,42,0.8)",
                   borderColor: 'rgba(226,133,42,0.8)',
                   fill: false,
                   data: reversal
               }, {
                   label: 'Incomplete',
                   hidden: true,
                   backgroundColor: "rgba(226,133,42,0.8)",
                   borderColor: 'rgba(226,133,42,0.8)',
                   fill: false,
                   data: incomplete
               }, {
                   label: 'Failed',
                   hidden: false,
                   backgroundColor: "rgb(255,0,0)",
                   borderColor: 'rgb(255,0,0)',
                   fill: false,
                   data: failed
               }, {
                   label: 'Failed Difference',
                   hidden: false,
                   backgroundColor: "rgb(200,180,120)",
                   borderColor: 'rgb(200,180,120)',
                   fill: false,
                   data: failedDiff
               }
            ]

        };

        var ctx = document.getElementById("canvas_nip_out").getContext("2d");
        window.myBar = new Chart(ctx, {
            type: 'line',
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
                    text: 'NIP Outgoing Transactions Chart'
                },
                scales: {
                    xAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Time'
                            }
                        }
                    ],
                    yAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Success'
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

    function trendingNipIncomingChartData(dateStr) {
        var success = [];
        var reversal = [];
        var incomplete = [];
        var pending = [];
        var failed = [];
        var failedDiff = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/CummNipIncomingTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { detailDate: dateStr },
            async: false,
            success: function (data) {
                for (var i = 0; i < data.length; i++) {
                    if (i < 1) {
                        var k = i;
                    } else {
                        k = i - 1;
                    }
                    success[i] = data[i]['SuccessCount'];
                    reversal[i] = data[i]['ReversedCount'];
                    incomplete[i] = data[i]['IncompleteCount'];
                    pending[i] = data[i]['PendingCount'];
                    failed[i] = data[i]['FailureCount'];
                    failedDiff[i] = data[i]['FailureCount'] - data[k]['FailureCount'];
                    dateTime[i] = data[i]['DateTime'];
                }
            }
        });

        var trendingChartData = {
            labels: dateTime, //dateTime,
            datasets: [
                {
                    label: 'Success',
                    backgroundColor: "rgba(20,180,120,0.8)",
                    borderColor: 'rgba(20,180,120,0.8)',
                    fill: false,
                    data: success
                }, {
                    label: 'Reversal',
                    hidden: true,
                    backgroundColor: "rgba(226,133,42,0.8)",
                    borderColor: 'rgba(226,133,42,0.8)',
                    fill: false,
                    data: reversal
                }, {
                    label: 'Incomplete',
                    hidden: true,
                    backgroundColor: "rgba(226,133,42,0.8)",
                    borderColor: 'rgba(226,133,42,0.8)',
                    fill: false,
                    data: incomplete
                }, {
                    label: 'Pending',
                    hidden: true,
                    backgroundColor: "rgba(226,133,42,0.8)",
                    borderColor: 'rgba(226,133,42,0.8)',
                    fill: false,
                    data: pending
                }, {
                    label: 'Failed',
                    hidden: false,
                    backgroundColor: "rgb(255,0,0)",
                    borderColor: 'rgb(255,0,0)',
                    fill: false,
                    data: failed
                }, {
                    label: 'Failed Difference',
                    hidden: false,
                    backgroundColor: "rgb(200,180,120)",
                    borderColor: 'rgb(200,180,120)',
                    fill: false,
                    data: failedDiff
                }
            ]

        };

        var ctx = document.getElementById("canvas_nip_in").getContext("2d");
        window.myBar = new Chart(ctx, {
            type: 'line',
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
                    text: 'NIP Incoming Transactions Chart'
                },
                scales: {
                    xAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Time'
                            }
                        }
                    ],
                    yAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Success'
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

    function trendingNeftChartData(dateStr) {
        var success = [];
        var reversal = [];
        var incomplete = [];
        var failed = [];
        var failedDiff = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/CummNeftTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { detailDate: dateStr },
            async: false,
            success: function (data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    if (i < 1) {
                        var k = i;
                    } else {
                        k = i - 1;
                    }
                    success[i] = data[i]['SuccessCount'];
                    reversal[i] = data[i]['PendingCount'];
                    incomplete[i] = data[i]['IncompleteCount'];
                    failed[i] = data[i]['FailureCount'];
                    failedDiff[i] = data[i]['FailureCount'] - data[k]['FailureCount'];
                    dateTime[i] = data[i]['DateTime'];
                    //console.log('rib ' + rib);
                }
            }
        });

        var trendingChartData = {
            labels: dateTime, //dateTime,
            datasets: [
                {
                    label: 'Success',
                    backgroundColor: "rgba(20,180,120,0.8)",
                    borderColor: 'rgba(20,180,120,0.8)',
                    fill: false,
                    data: success
                }, {
                    label: 'Reversal',
                    hidden: true,
                    backgroundColor: "rgba(226,133,42,0.8)",
                    borderColor: 'rgba(226,133,42,0.8)',
                    fill: false,
                    data: reversal
                }, {
                    label: 'Incomplete',
                    hidden: true,
                    backgroundColor: "rgba(226,133,42,0.8)",
                    borderColor: 'rgba(226,133,42,0.8)',
                    fill: false,
                    data: incomplete
                }, {
                    label: 'Failed',
                    hidden: false,
                    backgroundColor: "rgb(255,0,0)",
                    borderColor: 'rgb(255,0,0)',
                    fill: false,
                    data: failed
                }, {
                    label: 'FailedDiff',
                    hidden: false,
                    backgroundColor: "rgb(200,180,120)",
                    borderColor: 'rgb(200,180,120)',
                    fill: false,
                    data: failedDiff
                }
            ]

        };

        var ctx = document.getElementById("canvas_neft").getContext("2d");
        window.myBar = new Chart(ctx, {
            type: 'line',
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
                    text: 'Neft Transactions Chart'
                },
                scales: {
                    xAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Time'
                            }
                        }
                    ],
                    yAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Success'
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

    function trendingAtmChartData(dateStr) {
        var success = [];
        var failed = [];
        var failedDiff = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/CummAtmTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { detailDate: dateStr },
            async: false,
            success: function (data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    if (i < 1) {
                        var k = i;
                    } else {
                        k = i - 1;
                    }
                    success[i] = data[i]['SuccessCount'];
                    failed[i] = data[i]['FailureCount'];
                    failedDiff[i] = data[i]['FailureCount'] - data[k]['FailureCount'];
                    dateTime[i] = data[i]['DateTime'];
                    //console.log('rib ' + rib);
                }
            }
        });

        var trendingChartData = {
            labels: dateTime, //dateTime,
            datasets: [
                {
                    label: 'Success',
                    backgroundColor: "rgba(20,180,120,0.8)",
                    borderColor: 'rgba(20,180,120,0.8)',
                    fill: false,
                    data: success
                }, {
                    label: 'Failed',
                    hidden: false,
                    backgroundColor: "rgb(255,0,0)",
                    borderColor: 'rgb(255,0,0)',
                    fill: false,
                    data: failed
                }, {
                    label: 'Failed Difference',
                    hidden: false,
                    backgroundColor: "rgb(200,180,120)",
                    borderColor: 'rgb(200,180,120)',
                    fill: false,
                    data: failedDiff
                }
            ]

        };

        var ctx = document.getElementById("canvas_atm").getContext("2d");
        window.myBar = new Chart(ctx, {
            type: 'line',
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
                    text: 'Atm Transactions Chart'
                },
                scales: {
                    xAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Time'
                            }
                        }
                    ],
                    yAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Success'
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

    function trendingPosChartData(dateStr) {
        var success = [];
        var failed = [];
        var failedDiff = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/CummPosTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { detailDate: dateStr },
            async: false,
            success: function (data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    if (i < 1) {
                        var k = i;
                    } else {
                        k = i - 1;
                    }
                    success[i] = data[i]['SuccessCount'];
                    failed[i] = data[i]['FailureCount'];
                    failedDiff[i] = data[i]['FailureCount'] - data[k]['FailureCount'];
                    dateTime[i] = data[i]['DateTime'];
                    //console.log('rib ' + rib);
                }
            }
        });

        var trendingChartData = {
            labels: dateTime, //dateTime,
            datasets: [
                {
                    label: 'Success',
                    backgroundColor: "rgba(20,180,120,0.8)",
                    borderColor: 'rgba(20,180,120,0.8)',
                    fill: false,
                    data: success
                }, {
                    label: 'Failed',
                    hidden: false,
                    backgroundColor: "rgb(255,0,0)",
                    borderColor: 'rgb(255,0,0)',
                    fill: false,
                    data: failed
                }, {
                    label: 'Failed Difference',
                    hidden: false,
                    backgroundColor: "rgb(200,180,120)",
                    borderColor: 'rgb(200,180,120)',
                    fill: false,
                    data: failedDiff
                }
            ]

        };

        var ctx = document.getElementById("canvas_pos").getContext("2d");
        window.myBar = new Chart(ctx, {
            type: 'line',
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
                    text: 'POS Transactions Chart'
                },
                scales: {
                    xAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Time'
                            }
                        }
                    ],
                    yAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Success'
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

    function trendingWebChartData(dateStr) {
        var success = [];
        var failed = [];
        var failedDiff = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/CummWebTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { detailDate: dateStr },
            async: false,
            success: function (data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    if (i < 1) {
                        var k = i;
                    } else {
                        k = i - 1;
                    }
                    success[i] = data[i]['SuccessCount'];
                    failed[i] = data[i]['FailureCount'];
                    failedDiff[i] = data[i]['FailureCount'] - data[k]['FailureCount'];
                    dateTime[i] = data[i]['DateTime'];
                    //console.log('rib ' + rib);
                }
            }
        });

        var trendingChartData = {
            labels: dateTime, //dateTime,
            datasets: [
                {
                    label: 'Success',
                    backgroundColor: "rgba(20,180,120,0.8)",
                    borderColor: 'rgba(20,180,120,0.8)',
                    fill: false,
                    data: success
                }, {
                    label: 'Failed',
                    hidden: false,
                    backgroundColor: "rgb(255,0,0)",
                    borderColor: 'rgb(255,0,0)',
                    fill: false,
                    data: failed
                }, {
                    label: 'Failed Difference',
                    hidden: false,
                    backgroundColor: "rgb(200,180,120)",
                    borderColor: 'rgb(200,180,120)',
                    fill: false,
                    data: failedDiff
                }
            ]

        };

        var ctx = document.getElementById("canvas_web").getContext("2d");
        window.myBar = new Chart(ctx, {
            type: 'line',
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
                    text: 'Web Transactions Chart'
                },
                scales: {
                    xAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Time'
                            }
                        }
                    ],
                    yAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Success'
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

    function trendingNapsChartData(dateStr) {
        var success = [];
        var pending = [];
        var failed = [];
        var failedDiff = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/CummNapsTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { detailDate: dateStr },
            async: false,
            success: function (data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    if (i < 1) {
                        var k = i;
                    } else {
                        k = i - 1;
                    }
                    success[i] = data[i]['SuccessCount'];
                    pending[i] = data[i]['PendingCount'];
                    failed[i] = data[i]['FailureCount'];
                    failedDiff[i] = data[i]['FailureCount'] - data[k]['FailureCount'];
                    dateTime[i] = data[i]['DateTime'];
                    //console.log('rib ' + rib);
                }
            }
        });

        var trendingChartData = {
            labels: dateTime, //dateTime,
            datasets: [
                {
                    label: 'Success',
                    backgroundColor: "rgba(20,180,120,0.8)",
                    borderColor: 'rgba(20,180,120,0.8)',
                    fill: false,
                    data: success
                }, {
                    label: 'Pending',
                    hidden: true,
                    backgroundColor: "rgba(226,133,42,0.8)",
                    borderColor: 'rgba(226,133,42,0.8)',
                    fill: false,
                    data: pending
                }, {
                    label: 'Failed',
                    hidden: false,
                    backgroundColor: "rgb(255,0,0)",
                    borderColor: 'rgb(255,0,0)',
                    fill: false,
                    data: failed
                }, {
                    label: 'Failed Difference',
                    hidden: false,
                    backgroundColor: "rgb(200,180,120)",
                    borderColor: 'rgb(200,180,120)',
                    fill: false,
                    data: failedDiff
                }
            ]

        };

        var ctx = document.getElementById("canvas_naps").getContext("2d");
        window.myBar = new Chart(ctx, {
            type: 'line',
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
                    text: 'Naps Transactions Chart'
                },
                scales: {
                    xAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Time'
                            }
                        }
                    ],
                    yAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Success'
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

    function trendingEbillsChartData(dateStr) {
        var success = [];
        var pending = [];
        var reversed = [];
        var failed = [];
        var failedDiff = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/CummEbillsTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { detailDate: dateStr },
            async: false,
            success: function (data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    if (i < 1) {
                        var k = i;
                    } else {
                        k = i - 1;
                    }
                    success[i] = data[i]['SuccessCount'];
                    pending[i] = data[i]['PendingCount'];
                    reversed[i] = data[i]['ReversedCount'];
                    failed[i] = data[i]['FailureCount'];
                    failedDiff[i] = data[i]['FailureCount'] - data[k]['FailureCount'];
                    dateTime[i] = data[i]['DateTime'];
                    //console.log('rib ' + rib);
                }
            }
        });

        var trendingChartData = {
            labels: dateTime, //dateTime,
            datasets: [
                {
                    label: 'Success',
                    backgroundColor: "rgba(20,180,120,0.8)",
                    borderColor: 'rgba(20,180,120,0.8)',
                    fill: false,
                    data: success
                }, {
                    label: 'Pending',
                    hidden: true,
                    backgroundColor: "rgba(226,133,42,0.8)",
                    borderColor: 'rgba(226,133,42,0.8)',
                    fill: false,
                    data: pending
                }, {
                    label: 'Reversed',
                    hidden: true,
                    backgroundColor: "rgba(226,133,42,0.8)",
                    borderColor: 'rgba(226,133,42,0.8)',
                    fill: false,
                    data: reversed
                }, {
                    label: 'Failed',
                    hidden: false,
                    backgroundColor: "rgb(255,0,0)",
                    borderColor: 'rgb(255,0,0)',
                    fill: false,
                    data: failed
                }, {
                    label: 'Failed Difference',
                    hidden: false,
                    backgroundColor: "rgb(200,180,120)",
                    borderColor: 'rgb(200,180,120)',
                    fill: false,
                    data: failedDiff
                }
            ]

        };

        var ctx = document.getElementById("canvas_ebills").getContext("2d");
        window.myBar = new Chart(ctx, {
            type: 'line',
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
                    text: 'Ebills Transactions Chart'
                },
                scales: {
                    xAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Time'
                            }
                        }
                    ],
                    yAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Success'
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

    function trendingFlexBranchChartData(dateStr) {
        var success = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/CummFlexBranchTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { detailDate: dateStr },
            async: false,
            success: function (data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    success[i] = data[i]['SuccessCount'];
                    dateTime[i] = data[i]['DateTime'];
                    //console.log('rib ' + rib);
                }
            }
        });

        var trendingChartData = {
            labels: dateTime, //dateTime,
            datasets: [
                {
                    label: 'Success',
                    backgroundColor: "rgba(20,180,120,0.8)",
                    borderColor: 'rgba(20,180,120,0.8)',
                    fill: false,
                    data: success
                }
            ]

        };

        var ctx = document.getElementById("canvas_flexbranch").getContext("2d");
        window.myBar = new Chart(ctx, {
            type: 'line',
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
                    text: 'FlexBranch Transactions Chart'
                },
                scales: {
                    xAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Time'
                            }
                        }
                    ],
                    yAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Success'
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
    function trendingNameEnquiryChartData(dateStr) {
        var success = [];
        var pending = [];
        var failed = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/CummNameEnquiryTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { detailDate: dateStr },
            async: false,
            success: function (data) {
                console.log(data);
                for (var i = 0; i < data.length; i++) {
                    success[i] = data[i]['Success'];
                    pending[i] = data[i]['Pending'];
                    failed[i] = data[i]['Failed'];
                    dateTime[i] = data[i]['Time'];
                }
            }
        });

        var trendingChartData = {
            labels: dateTime, //dateTime,
            datasets: [
                {
                    label: 'Success',
                    backgroundColor: "rgba(20,180,120,0.8)",
                    borderColor: 'rgba(20,180,120,0.8)',
                    fill: false,
                    data: success
                },{
                    label: 'Pending',
                    backgroundColor: "rgba(220,180,120,0.8)",
                    borderColor: 'rgba(220,180,120,0.8)',
                    fill: false,
                    data: pending
                },
                {
                    label: 'Failed',
                    backgroundColor: "rgba(255,0,0,0.5)",
                    borderColor: 'rgba(255,0,0,0.5)',
                    fill: false,
                    data: failed
                }
            ]

        };

        var ctx = document.getElementById("canvas_name_enquiry").getContext("2d");
        window.myBar = new Chart(ctx, {
            type: 'line',
            data: trendingChartData,
            options: {
                // Elements options apply to all of the options unless overridden in a dataset
                // In this case, we are setting the border of each bar to be 2px wide and green
                elements: {
                    rectangle: {
                        borderWidth: 2,
                        borderSkipped: 'bottom'
                    }
                },
                responsive: true,
                legend: {
                    position: 'bottom',
                },
                title: {
                    display: true,
                    text: 'Name Enquiry Chart'
                },
                scales: {
                    xAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Time'
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
                                suggestedMax: 10,

                            }
                        }
                    ]
                }
            }
        });
    }

    function trendingMerchantOnboardingChartData() {
        var success = [];
        var count = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/MerchantOnboardingTrend",
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
                    label: 'MerchantCount',
                    backgroundColor: "rgba(20,180,120,0.8)",
                    borderColor: 'rgba(20,180,120,0.8)',
                    fill: false,
                    data: success
                }
            ]

        };

        var ctx = document.getElementById("merchant_onboarding").getContext("2d");
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
                    text: 'Merchant Onboarding Chart'
                },
                scales: {
                    xAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Date Approved'
                            }
                        }
                    ],
                    yAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Merchant Count'
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

    trendingMobileChartData(dateStr);
    trendingTopupChartData(dateStr);
    trendingInterBankChartData(dateStr);
    trendingXpressBoardingChartData(dateStr);
    trendingRibChartData(dateStr);
    trendingNipOutChartData(dateStr);
    trendingNipIncomingChartData(dateStr);
    trendingNeftChartData(dateStr);
    trendingAtmChartData(dateStr);
    trendingPosChartData(dateStr);
    trendingWebChartData(dateStr);
    trendingNapsChartData(dateStr);
    trendingEbillsChartData(dateStr);
    trendingFlexBranchChartData(dateStr);
    trendingNameEnquiryChartData(dateStr);
    //trendingMerchantOnboardingChartData();

    function loadGraphForDate() {
        console.log("loadGraphForDate call");

        trendingMobileChartData(dateStr);
        trendingTopupChartData(dateStr);
        trendingInterBankChartData(dateStr);
        trendingXpressBoardingChartData(dateStr);

        trendingRibChartData(dateStr);
        trendingNipOutChartData(dateStr);
        trendingNipIncomingChartData(dateStr);
        trendingNeftChartData(dateStr);

        trendingAtmChartData(dateStr);
        trendingPosChartData(dateStr);
        trendingWebChartData(dateStr);

        trendingNapsChartData(dateStr);
        trendingEbillsChartData(dateStr);
        trendingNameEnquiryChartData(dateStr);
        trendingFlexBranchChartData(dateStr);
        //trendingMerchantOnboardingChartData();
    }

    document.getElementById('detail_date').onclick = function () { loadGraphForDate(); };
    //console.log(dateStr);
    //var btnDate =
    //document.getElementById("detail_date").onclick(loadGraphForDate(dateStr));
    //btnDate.onclick = loadGraphForDate(dateStr);

};