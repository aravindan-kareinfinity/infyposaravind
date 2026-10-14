
var app = angular.module('InfinityLab');

app.controller('FBReportController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'Excel', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, Excel, $timeout) {


        $scope.QuestionList = restService.Get('QuestionService/SearchAllQuestions', true).then(function (results) {
            $scope.QuestionList = results;
        });

        $scope.QuestionChanged = function () {
            selectedquestion
            if (typeof ($scope.questionid) == 'undefined') return;

            var selectedquestion = $scope.QuestionList.find(function (item) {
                return item.id == $scope.questionid;
            });

            if (typeof (selectedquestion) == 'undefined') return;
            $scope.AnswerList = selectedquestion.answerlist;
        }

        $scope.panelheight = { 'height': '500px' };

        $scope.Search = function () {

            var request = {
                "item": $scope.Report,
                "securitytoken": "String content"
            };

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "FeedbackService/SearchReport",
                data: srequest
            }).success(function (result) {

                if (result == null || result == 'undefined') {
                    $rootScope.Toaster.Info("No Results found.");
                    return;
                }

                if ($scope.Report.rpttype == 'SUMMARY') {
                    if (result.summaryitems == null || result.summaryitems == 'undefined' || 
                        result.summaryitems.length == 0) {
                        $rootScope.Toaster.Info("No Results found.");
                        return;
                    }                    
                    $scope.ShowSummary(result);
                }
                else {
                    if (result.detailitems == null || result.detailitems == 'undefined' ||
                        result.detailitems.length == 0) {
                        $rootScope.Toaster.Info("No Results found.");
                        return;
                    }
                    $scope.ShowDetail(result);
                }

            }).error(function (result) {
            });
        }


        $scope.rgrid = null;
        $scope.ShowDetail = function (result) {

            if ($scope.rgrid != null)
                $scope.rgrid.destroy();

            var rgrid = new InfyGrid('gridContainer', '400px', '100%', true, false, true, 30);
            rgrid.options.showHeaderRow = true;

            rgrid.EnableSerial(result.detailitems);

            rgrid.AddFormatedColumn('feedbackdate', 'Date', 80, 'dd-MM-yyyy');
            rgrid.AddSimpleColumn('mobileno', 'Mobileno', 200);
            rgrid.AddSimpleColumn('name', 'Name', 100);
            rgrid.AddSimpleColumn('overrallrating', 'Rating', 100);
            rgrid.AddCustomColumn('hascomplaint', 'Complaint', 40, function (item) {
                return item.hascomplaint ? "Yes" : "No";
            });
            rgrid.AddCustomColumn('hassugession', 'Sugession', 40, function (item) {
                return item.hassugession ? "Yes" : "No";
            });

            $.each(result.columns, function (k, v) {

                var column = rgrid.AddCustomColumn('Ans-' + k, v, 100, function (item, col) {

                    var ans = item.qafeedback.find(function (fitem) {
                        return fitem.question == col.internal;
                    })

                    if (typeof (ans) == 'undefined' || ans == null)
                        return '';
                    return ans.answer;
                });

                column.internal = v;
            });

            rgrid.AddSimpleColumn('complaint', 'Complaint', 100);
            rgrid.AddSimpleColumn('sugession', 'Sugession', 100);

            rgrid.Initialize();
            $scope.rgrid = rgrid;
            $scope.rgrid.UpdateData(result.detailitems);
        }

        $scope.ShowSummary = function (result) {

            if ($scope.rgrid != null)
                $scope.rgrid.destroy();

            var rgrid = new InfyGrid('gridContainer', '400px', '100%', true, false, true, 30);
            rgrid.options.showHeaderRow = true;

            rgrid.EnableSerial(result.summaryitems);
            rgrid.AddSimpleColumn('question', 'Question', 200);
            rgrid.AddSimpleColumn('answer', 'Answer', 100);
            rgrid.AddSimpleColumn('count', 'Count', 100);
            rgrid.Initialize();
            $scope.rgrid = rgrid;
            $scope.rgrid.UpdateData(result.summaryitems);
        }

        $scope.igrid = null;
        $scope.ConfigCreteria = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridCreteria', '400px', '100%', true, false, true, 250);
            igrid.options.showHeaderRow = true;

            igrid.EnableSerial($scope.Report.List);
            igrid.AddSimpleColumn('question', 'question', 200);
            igrid.AddSimpleColumn('answer', 'answer', 100);

            var commands = "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"
            igrid.AddButtonColumn('action', 'ACTION', 50, commands, function (e, args) {
                $scope.$apply(function () {
                    $scope.Report.List.splice($scope.Report.List.indexOf(e.item), 1);
                    $scope.igrid.UpdateData($scope.Report.List);
                });
            });

            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.Report.List);

        }

        $scope.AddClicked = function () {
            $scope.Report.List = $scope.Report.List || [];

            $scope.Report.List.push({
                'questionid': $scope.questionid,
                'question': $scope.QuestionList.find(function (item) {
                    return item.id == $scope.questionid;
                                    }).question,
                'answerid': $scope.answerid,
                'answer': $scope.AnswerList.find(function (item) {
                    return item.id == $scope.answerid;
                }).answer
            });
            $scope.ConfigCreteria();
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = ''

            if (tableid == 'customerfeedback')
                subtitle = 'CUSTOMER FEEDBACK';


            $scope.exportHref = infygrid.ExportExcel(infygrid.grid, title, subtitle);
            if ($scope.exportHref != null) {
                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = filename + ".xls";
                    link.href = $scope.exportHref;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
        }

        $timeout(function () {
            var available = (typeof ($rootScope.windowSize) != 'undefined' && typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
            if (available > 0) {
                $scope.panelheight.height = '' + (available - 100) + 'px';
            }
            $scope.setFocusfirst();
        }, 100);

    }]);