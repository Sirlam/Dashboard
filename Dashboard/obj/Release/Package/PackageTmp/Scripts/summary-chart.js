$(document).ready(function () {

    var address = "/dashboard/atm";
    //var address = "/Atm";
    
    function trendingAtmChartData() {
        var gold = [];
        var silver = [];
        var bronze = [];
        var dateTime = [];
        $.ajax({
            type: "GET",
            url: address + "/SummaryTrends",
            contentType: "application/json",
            datatype: 'json',
            async: false,
            success: function (data) {
                for (var i = 0; i < data.length; i++) {
                    gold[i] = data[i]['GoldCount'];
                    silver[i] = data[i]['SilverCount'];
                    bronze[i] = data[i]['BronzeCount'];
                    dateTime[i] = data[i]['DateTime'];
                }
            }
        });

        var trendingChartData = {
            labels: dateTime,
            datasets: [
                {
                    hidden: false,
                    label: 'Gold',
                    backgroundColor: "rgba(255,215,0,1)",
                    borderColor: 'rgba(255,215,0,1)',
                    fill: false,
                    data: gold
                },  {
                    hidden: false,
                    label: 'Silver',
                    backgroundColor: "rgb(192,192,192)",
                    borderColor: 'rgb(192,192,192)',
                    fill: false,
                    data: silver
                },  {
                    hidden: false,
                    label: 'Bronze',
                    backgroundColor: "rgb(205,127,50)",
                    borderColor: 'rgb(205,127,50)',
                    fill: false,
                    data: bronze
                }
            ]

        };

        var ctx = document.getElementById("canvas_summary").getContext("2d");
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
                    text: 'Atm Performance Chart'
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
                                labelString: 'In Service %'
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


    trendingAtmChartData();


});