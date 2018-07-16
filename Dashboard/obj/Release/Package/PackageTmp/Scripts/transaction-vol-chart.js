window.onload = function () {

    var address = "/dashboard/Volume";
    //var address = "/Volume";
    var dateStr = document.getElementById("InputDate").value;

    function trendingMacallaVolChartData(dateStr) {
        var success = [];
        var failed = [];
        var pending = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/VolMacallaTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { detailDate: dateStr },
            async: false,
            success: function (data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    success[i] = data[i]['SuccessCount'];
                    failed[i] = data[i]['FailureCount'];
                    pending[i] = data[i]['PendingCount'];
                    dateTime[i] = data[i]['DateTime'];
                    //console.log('rib ' + rib);
                }
            }
        });

        var trendingChartData = {
            labels: dateTime, //dateTime,
            datasets: [
                {
                    hidden: false,
                    label: 'Success',
                    backgroundColor: "rgba(0,0,255,1)",
                    borderColor: 'rgba(0,0,255,1)',
                    fill: false,
                    data: success
                }, {
                    hidden: true,
                    label: 'Pending',
                    backgroundColor: "rgba(226,133,42,0.8)",
                    borderColor: 'rgba(226,133,42,0.8)',
                    fill: false,
                    data: pending
                }, {
                    hidden: false,
                    label: 'Failed',
                    backgroundColor: "rgb(255,0,0)",
                    borderColor: 'rgb(255,0,0)',
                    fill: false,
                    data: failed
                }
            ]

        };

        var ctx = document.getElementById("canvas_macalla_vol").getContext("2d");
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
                    text: 'Macalla Transactions Chart'
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
                                labelString: 'Transaction Volume'
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

    function trendingRibVolChartData(dateStr) {
        var success = [];
        var reversed = [];
        var pending = [];
        var failed = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/VolRibTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { detailDate: dateStr },
            async: false,
            success: function (data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    success[i] = data[i]['SuccessCount'];
                    reversed[i] = data[i]['ReversedCount'];
                    pending[i] = data[i]['PendingCount'];
                    failed[i] = data[i]['FailureCount'];
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
                }
            ]

        };

        var ctx = document.getElementById("canvas_rib_vol").getContext("2d");
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
                                labelString: 'Transaction Volume'
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

    function trendingNipOutVolChartData(dateStr) {
        var success = [];
        var reversal = [];
        var incomplete = [];
        var failed = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/VolNipOutgoingTrend",
            contentType: "application/json",
            datatype: 'json',
            async: false,
            data: { detailDate: dateStr },
            success: function (data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    success[i] = data[i]['SuccessCount'];
                    reversal[i] = data[i]['ReversedCount'];
                    incomplete[i] = data[i]['IncompleteCount'];
                    failed[i] = data[i]['FailureCount'];
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
               }
            ]

        };

        var ctx = document.getElementById("canvas_nip_out_vol").getContext("2d");
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
                                labelString: 'Transaction Volume'
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

    function trendingNipIncomingVolChartData(dateStr) {
        var success = [];
        var reversal = [];
        var incomplete = [];
        var pending = [];
        var failed = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/VolNipIncomingTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { detailDate: dateStr },
            async: false,
            success: function (data) {
                for (var i = 0; i < data.length; i++) {
                    success[i] = data[i]['SuccessCount'];
                    reversal[i] = data[i]['ReversedCount'];
                    incomplete[i] = data[i]['IncompleteCount'];
                    pending[i] = data[i]['PendingCount'];
                    failed[i] = data[i]['FailureCount'];
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
                }
            ]

        };

        var ctx = document.getElementById("canvas_nip_in_vol").getContext("2d");
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
                                labelString: 'Transaction Volume'
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

    function trendingNeftVolChartData(dateStr) {
        var success = [];
        var reversal = [];
        var incomplete = [];
        var failed = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/VolNeftTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { detailDate: dateStr },
            async: false,
            success: function (data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    success[i] = data[i]['SuccessCount'];
                    reversal[i] = data[i]['PendingCount'];
                    incomplete[i] = data[i]['IncompleteCount'];
                    failed[i] = data[i]['FailureCount'];
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
                }
            ]

        };

        var ctx = document.getElementById("canvas_neft_vol").getContext("2d");
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
                                labelString: 'Transaction Volume'
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

    function trendingAtmVolChartData(dateStr) {
        var success = [];
        var failed = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/VolAtmTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { detailDate: dateStr },
            async: false,
            success: function (data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    success[i] = data[i]['SuccessCount'];
                    failed[i] = data[i]['FailureCount'];
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
                }
            ]

        };

        var ctx = document.getElementById("canvas_atm_vol").getContext("2d");
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
                                labelString: 'Transaction Volume'
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

    function trendingPosVolChartData(dateStr) {
        var success = [];
        var failed = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/VolPosTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { detailDate: dateStr },
            async: false,
            success: function (data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    success[i] = data[i]['SuccessCount'];
                    failed[i] = data[i]['FailureCount'];
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
                }
            ]

        };

        var ctx = document.getElementById("canvas_pos_vol").getContext("2d");
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
                                labelString: 'Transaction Volume'
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

    function trendingWebVolChartData(dateStr) {
        var success = [];
        var failed = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/VolWebTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { detailDate: dateStr },
            async: false,
            success: function (data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    success[i] = data[i]['SuccessCount'];
                    failed[i] = data[i]['FailureCount'];
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
                }
            ]

        };

        var ctx = document.getElementById("canvas_web_vol").getContext("2d");
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
                                labelString: 'Transaction Volume'
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

    function trendingNapsVolChartData(dateStr) {
        var success = [];
        var pending = [];
        var failed = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/VolNapsTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { detailDate: dateStr },
            async: false,
            success: function (data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    success[i] = data[i]['SuccessCount'];
                    pending[i] = data[i]['PendingCount'];
                    failed[i] = data[i]['FailureCount'];
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
                }
            ]

        };

        var ctx = document.getElementById("canvas_naps_vol").getContext("2d");
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
                                labelString: 'Transaction Volume'
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

    function trendingEbillsVolChartData(dateStr) {
        var success = [];
        var pending = [];
        var reversed = [];
        var failed = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/VolEbillsTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { detailDate: dateStr },
            async: false,
            success: function (data) {
                //console.log(data);
                for (var i = 0; i < data.length; i++) {
                    success[i] = data[i]['SuccessCount'];
                    pending[i] = data[i]['PendingCount'];
                    reversed[i] = data[i]['ReversedCount'];
                    failed[i] = data[i]['FailureCount'];
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
                }
            ]

        };

        var ctx = document.getElementById("canvas_ebills_vol").getContext("2d");
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
                                labelString: 'Transaction Volume'
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

    trendingMacallaVolChartData(dateStr);
    trendingRibVolChartData(dateStr);
    trendingNipOutVolChartData(dateStr);
    trendingNipIncomingVolChartData(dateStr);
    trendingNeftVolChartData(dateStr);
    trendingAtmVolChartData(dateStr);
    trendingPosVolChartData(dateStr);
    trendingWebVolChartData(dateStr);
    trendingNapsVolChartData(dateStr);
    trendingEbillsVolChartData(dateStr);

    function loadGraphForDate() {
        console.log("loadGraphForDate call");
        trendingMacallaVolChartData(dateStr);
        trendingRibVolChartData(dateStr);
        trendingNipOutVolChartData(dateStr);
        trendingNipIncomingVolChartData(dateStr);
        trendingNeftVolChartData(dateStr);

        trendingAtmVolChartData(dateStr);
        trendingPosVolChartData(dateStr);
        trendingWebVolChartData(dateStr);

        trendingNapsVolChartData(dateStr);
        trendingEbillsVolChartData(dateStr);
    }

    document.getElementById('detail_date').onclick = function() { loadGraphForDate(); };
    //console.log(dateStr);
    //var btnDate =
    //document.getElementById("detail_date").onclick(loadGraphForDate(dateStr));
    //btnDate.onclick = loadGraphForDate(dateStr);

};