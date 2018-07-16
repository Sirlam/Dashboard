window.onload = function () {

    var address = "/dashboard/MasterPass";
    //var address = "/MasterPass";

    function trendingMerchantOnboardingChartData2() {
        var count = [];
        var cs = [];
        var cm = [];
        var db = [];
        var others = [];
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
                    cs[i] = data[i]['Cs'];
                    cm[i] = data[i]['Cm'];
                    db[i] = data[i]['Db'];
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
                    label: 'Merchant Count',
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
                }, {
                    label: 'DB',
                    backgroundColor: "rgba(0,200,200,0.8)",
                    borderColor: 'rgba(0,200,200,0.8)',
                    fill: false,
                    data: db
                }, {
                    label: 'Others',
                    backgroundColor: "rgba(200,2,2,0.8)",
                    borderColor: 'rgba(200,2,2,0.8)',
                    fill: false,
                    data: others
                }
            ]

        };

        var ctx = document.getElementById("merchant_onboarding2").getContext("2d");
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
    trendingMerchantOnboardingChartData2();

};