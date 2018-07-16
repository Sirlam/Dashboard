window.onload = function () {
    Array.prototype.contains = function (v) {
        for (var i = 0; i < this.length; i++) {
            if (this[i] === v) return true;
        }
        return false;
    };
    Array.prototype.unique = function () {
        var arr = [];
        for (var i = 0; i < this.length; i++) {
            if (!arr.contains(this[i])) {
                arr.push(this[i]);
            }
        }
        return arr;
    };

    var dateStr = document.getElementById("Days1").value;
    var application = document.getElementById("SelectedApplication1").value;

    var address = "/dashboard/Analysis";
    //var address = "/Analysis";
    function trendingAnalysisChartData(dateStr, application) {
        var count = [];
        var transCode = [];
        var codeDescription = [];
        var date = [];
        var data;
        var error21 = [];
        var error98 = [];
        var error99 = [];
        var error06 = [];
        var error91 = [];
        var error96 = [];
        var error01 = [];
        var error25 = [];
        var i21, i98, i99, i06, i91, i96, i01, i25, iFt, iPt, iN, iIB, iIA;
        var errorTopUpLimitExceeded = [];
        var errorUnableToVend = [];
        var errorProcessingTransaction = [];
        var errorNull = [];
        var errorFailTransaction = [];
        var errorReversalFailure = [];
        var errorInsufficientBalance = [];
        var errorInvalidAccount = [];

        $.ajax({
            type: "GET",
            url: address + "/AnalysisTrend",
            contentType: "application/json",
            datatype: 'json',
            data: { selectedApplication: application, days: dateStr },
            async: false,
            success: function (dataO) {
                console.log(dataO);
                data = new Array(dataO.length);
                for (var i = 0; i < dataO.length; i++) {
                    data[i] = new Array(4);
                    data[i][0] = dataO[i]['DateTime'];
                    data[i][1] = dataO[i]['TransCode'];
                    data[i][2] = dataO[i]['CodeDescription'];
                    data[i][3] = dataO[i]['Count'];

                    transCode[i] = dataO[i]['TransCode'];
                    date[i] = dataO[i]['DateTime'];
                }
            }
        });
        //console.log(data);
        var x = new Array(date.unique().length + 1);
        transCode.unique().forEach(function gh(dt, h, gh) {
            if (dt == '21') {
                i21 = h + 1;
            }
            if (dt == '98') {
                i98 = h + 1;
            }
            if (dt == '99') {
                i99 = h + 1;
            }
            if (dt == '06') {
                i06 = h + 1;
            }
            if (dt == '91') {
                i91 = h + 1;
            }
            if (dt == '96') {
                i96 = h + 1;
            }
            if (dt == '01') {
                i01 = h + 1;
            }
            if (dt == '25') {
                i25 = h + 1;
            }
            if (dt == 'error processing transaction') {
                iPt = h + 1;
            }
            if (dt == 'failed transaction') {
                iFt = h + 1;
            }
            if (dt == 'insufficient balance' || dt == 'insufficientbalance') {
                iIB = h + 1;
            }
            if (dt == 'invalid account') {
                iIA = h + 1;
            }
            if (dt == 'null' || dt == '') {
                iN = h + 1;
            }
        });
        date.unique().forEach(function dateData(dt, i, dat) {
            x[i + 1] = new Array(transCode.unique().length + 1);
            error21[i] = 0;
            error98[i] = 0;
            error99[i] = 0;
            error06[i] = 0;
            error91[i] = 0;
            error96[i] = 0;
            error01[i] = 0;
            error25[i] = 0;

            errorTopUpLimitExceeded[i] = 0;
            errorUnableToVend[i] = 0;
            errorProcessingTransaction[i] = 0;
            errorNull[i] = 0;
            errorReversalFailure[i] = 0;
            errorInsufficientBalance[i] = 0;
            errorInvalidAccount[i] = 0;
            errorFailTransaction[i] = 0;

            transCode.unique().forEach(function callback(cde, j, code) {
                if (i == 0) {
                    x[0] = new Array(transCode.unique().length + 1);
                    x[0][0] = "Date";
                }
                x[0][j + 1] = cde;
                data.forEach(function td(arr) {
                    if (arr[0] == dt && arr[1] == cde) {
                        x[i + 1][0] = dt;
                        x[i + 1][j + 1] = arr[3];

                        if (i21 == j + 1)
                            error21[i] += arr[3];

                        if (i98 == j + 1)
                            error98[i] += arr[3];

                        if (i99 == j + 1)
                            error99[i] += arr[3];

                        if (i06 == j + 1)
                            error06[i] += arr[3];

                        if (i91 == j + 1)
                            error91[i] += arr[3];

                        if (i96 == j + 1)
                            error96[i] += arr[3];

                        if (i01 == j + 1)
                            error01[i] += arr[3];

                        if (i25 == j + 1)
                            error25[i] += arr[3];
                        if (iFt == j + 1)
                            errorFailTransaction[i] += arr[3];
                        if (iPt == j + 1)
                            errorProcessingTransaction[i] += arr[3];
                        if (iN == j + 1)
                            errorNull[i] += arr[3];
                        if (iIB == j + 1)
                            errorInsufficientBalance[i] += arr[3];
                        if (iIA == j + 1)
                            errorInvalidAccount[i] += arr[3];


                    }
                });

            });
        });

        console.log(iFt);
        console.log(iIA);
        console.log(iIB);
        console.log(iN);
        console.log(iPt);
        console.log(i96);
        console.log(i01);
        console.log(i25);
        console.log(errorFailTransaction);
        console.log(errorProcessingTransaction);
        console.log(errorNull);
        console.log(errorInsufficientBalance);
        console.log(errorInvalidAccount);
        console.log(error96);
        console.log(x);

        var ribChartData = {
            labels: date.unique(), //dateTime,
            datasets: [
                {
                    hidden: false,
                    label: 'Error 21',
                    backgroundColor: "rgba(226,133,42,0.8)",
                    borderColor: 'rgba(226,133,42,0.8)',
                    fill: false,
                    data: error21
                }, {
                    hidden: false,
                    label: 'Error 98',
                    backgroundColor: "rgb(255,0,0)",
                    borderColor: 'rgb(255,0,0)',
                    fill: false,
                    data: error98
                }, {
                    hidden: false,
                    label: 'Error 99',
                    backgroundColor: "rgb(0,0,180)",
                    borderColor: 'rgb(0,0,180)',
                    fill: false,
                    data: error99
                }

            ]

        };

        var fepChartData = {
            labels: date.unique(), //dateTime,
            datasets: [
                {
                    hidden: false,
                    label: 'Error 06',
                    backgroundColor: "rgba(20,20,20,0.8)",
                    borderColor: 'rgba(20,20,20,0.8)',
                    fill: false,
                    data: error06
                }, {
                    hidden: false,
                    label: 'Error 91',
                    backgroundColor: "rgba(200,200,0,0.8)",
                    borderColor: 'rgba(200,200,0,0.8)',
                    fill: false,
                    data: error91
                }, {
                    hidden: false,
                    label: 'Error 96',
                    backgroundColor: "rgba(128,0,128,0.8)",
                    borderColor: 'rgba(128,0,128,0.8)',
                    fill: false,
                    data: error96
                }

            ]

        };

        var mobileChartData = {
            labels: date.unique(), //dateTime,
            datasets: [
                //{
                //    hidden: false,
                //    label: 'TopUp Limit Exceeded',
                //    backgroundColor: "rgba(20,20,20,0.8)",
                //    borderColor: 'rgba(20,20,20,0.8)',
                //    fill: false,
                //    data: errorTopUpLimitExceeded
                //}, 
                {
                    hidden: false,
                    label: 'Fail Transaction',
                    backgroundColor: "rgba(200,200,0,0.8)",
                    borderColor: 'rgba(200,200,0,0.8)',
                    fill: false,
                    data: errorFailTransaction
                }, {
                    hidden: false,
                    label: 'error Processing Transaction',
                    backgroundColor: "rgba(128,0,128,0.8)",
                    borderColor: 'rgba(128,0,128,0.8)',
                    fill: false,
                    data: errorProcessingTransaction
                }, {
                    hidden: false,
                    label: 'Null',
                    backgroundColor: "rgba(200,0,180,0.8)",
                    borderColor: 'rgba(200,0,180,0.8)',
                    fill: false,
                    data: errorNull
                },  {
                    hidden: false,
                    label: 'Insufficient Balance',
                    backgroundColor: "rgba(255,0,0,0.8)",
                    borderColor: 'rgba(255,0,0,0.8)',
                    fill: false,
                    data: errorInsufficientBalance
                }, {
                    hidden: false,
                    label: 'Invalid Account',
                    backgroundColor: "rgba(180,180,180,0.8)",
                    borderColor: 'rgba(180,180,180,0.8)',
                    fill: false,
                    data: errorInvalidAccount
                }

            ]

        };

        var ribctx = document.getElementById("rib_canvas_analysis").getContext("2d");
        var fepctx = document.getElementById("fep_canvas_analysis").getContext("2d");
        var mobilectx = document.getElementById("mobile_canvas_analysis").getContext("2d");
        window.myBar = new Chart(ribctx, {
            type: 'line',
            data: ribChartData,
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
                    text: 'RIB'
                },
                scales: {
                    xAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Date'
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

        window.myBar = new Chart(fepctx, {
            type: 'line',
            data: fepChartData,
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
                    text: 'FEP'
                },
                scales: {
                    xAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Date'
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

        window.myBar = new Chart(mobilectx, {
            type: 'line',
            data: mobileChartData,
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
                    text: 'MOBILE'
                },
                scales: {
                    xAxes: [
                        {
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Date'
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

    trendingAnalysisChartData(dateStr, application);

    function loadGraphForDate() {
        trendingAnalysisChartData(dateStr, application);
    }

    document.getElementById('detail_submit').onclick = function () { loadGraphForDate(); };

    // function loadGraphForDate() {
    //     console.log("loadGraphForDate call");
    //     trendingMacallaChartData(dateStr);
    //}

    // document.getElementById('detail_date').onclick = function() { loadGraphForDate(); };
    //console.log(dateStr);
    //var btnDate =
    //document.getElementById("detail_date").onclick(loadGraphForDate(dateStr));
    //btnDate.onclick = loadGraphForDate(dateStr);

};