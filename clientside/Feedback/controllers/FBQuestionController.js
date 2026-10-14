
var app = angular.module('InfinityLab');

app.controller('FBQuestionController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'Excel', 'restService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, Excel, restService, $timeout) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.Questions = [];
        $scope.QuestionsCount = 0;
        $scope.panelheight = { 'height': '500px' };
        $scope.ChartList = [{ 'id': 0, 'name': 'No' },
        { 'id': 1, 'name': 'Yes' }];

        $scope.ChartPosition = [{ 'id': 0, 'name': 'None' },
        { 'id': 1, 'name': 'Left Side' },
        { 'id': 2, 'name': 'Right Side' }];

        $scope.Entity = {
            answerlist: [],
            question: '',
            isactive: true,
            chartposition: 0,
            charttype: 0,
            answercount: 0,
            code: '',
            id: 0
        };

        $http.get('QuestionService/SearchAllQuestions').then(function (results) {
            $scope.Questions = results.data;
            $scope.QuestionsCount = results.data.length - 1;
        });

        $scope.LastAnswerList = [];
        $scope.BuildAnswerList = function () {
            var count = parseInt($scope.Question.answercount);
            if (isNaN(count)) return;
            var newlist = [];
            var id = new Date().getTime();

            for (var i = 0; i < count; i++) {
                if ($scope.Question.answerlist.length > i) {
                    newlist.push($scope.Question.answerlist[i]);
                }
                else {
                    var found = false;
                    for (var j = 0; j < $scope.LastAnswerList.length; j++) {
                        if (newlist.indexOf($scope.LastAnswerList[j]) < 0) {
                            newlist.push(
                                $scope.LastAnswerList[j]
                            );
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        newlist.push({ 'id': id + i, 'answer': '', 'ack': false });
                }
            }

            for (var i = 0; i < $scope.Question.answerlist.length; i++) {
                if ($scope.Question.answerlist[i].answer != '' &&
                    $scope.LastAnswerList.indexOf($scope.Question.answerlist[i]) < 0) {
                    $scope.LastAnswerList.push($scope.Question.answerlist[i]);
                }
            }
            $scope.Question.answerlist.splice(0, $scope.Question.answerlist.length);
            $.each(newlist, function (k, v) {
                $scope.Question.answerlist.push(v);
            });
        }

        $scope.Question = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Question = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ErrorMessage = "";



        $scope.SearchCondition = {
            Questiontype: "",
            name: "",
            mobileno: "",
            loyeltyno: "",
            supportcredit: false,
            religionid: ""
        }
        $scope.New = function () {
            $scope.Question = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.SearchResult = [];
            $scope.ErrorMessage = "";
        }


        $scope.ViewResult = [];
        $scope.SearchView = function (id) {

            request = {
                "item": { id: id },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "QuestionService/SearchQuestionBills",
                data: srequest
            }).success(function (result) {
                $scope.totalbillvalue = 0;
                $scope.totalreward = 0;
                if (result != 'undefined' && result.length > 0) {
                    $scope.ViewResult = result;
                    $.each($scope.ViewResult, function (k, v) {
                        $scope.totalbillvalue += v.billvalue;
                        $scope.totalreward += (v.loyelty - v.adjusted);
                    });
                }
                else
                    $scope.ViewResult = [];
            }).error(function (result) {
            });

        }

        $scope.Save = function () {

            if ($scope.Question.code == null || $scope.Question.code == 0) {
                $scope['setFocuscode']();
                $rootScope.Toaster.Error("Please enter code");
                return;
            }
            if ($scope.Question.question == null || $scope.Question.question == "") {
                $scope['setFocusquestion']();
                $rootScope.Toaster.Error("Please fill out question");
                return;
            }

            if ($scope.Questions.length > 0)
                $scope.Question.index = $scope.Questions[$scope.Questions.length - 1].index;

            var request = {
                "Item": $scope.Question,
                "securitytoken": "String content"
            }

            $http({
                method: 'POST',
                url: "QuestionService/Save",
                data: request
            }).success(function (result) {

                if (result != null && result.id > 0 && (result.ErrorMessage == null || result.ErrorMessage == '')) {
                    $rootScope.Toaster.Info("Question Saved Successfully.!");
                    if ($scope.Questions.indexOf($scope.Question) >= 0) {
                        $scope.Question.version = result.version;
                        $scope.Question.modifiedon = result.modifiedon;
                        $scope.Question.modifiedby = result.modifiedby;
                    }
                    else {
                        $scope.Questions.push(result);
                    }
                    $scope.Question = (JSON.parse(JSON.stringify($scope.Entity)));
                    $scope.QuestionsCount = $scope.Questions.length - 1;
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
            });

        }


        $scope.Delete = function () {


            $.confirm({
                title: 'Delete',
                content: 'Do you want to delete ' + $scope.Question.code + "?",
                confirmButton: 'Yes',
                confirmButtonClass: 'btn-info',
                icon: 'fa fa-question-circle',
                animation: 'scale',
                animationClose: 'top',
                opacity: 0.5,
                buttons: {
                    formSubmit: {
                        text: 'Submit',
                        btnClass: 'btn-blue',
                        action: function () {

                            request = {
                                "Item": $scope.Question,
                                "SecurityToken": "String content"
                            }
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "QuestionService/Delete",
                                data: srequest
                            }).success(function (result) {

                                if (result) {
                                    $scope.ErrorMessage = "Successfully deleted";
                                    $scope.Questions.splice($scope.Questions.indexOf($scope.Question), 1);
                                    $scope.Question = (JSON.parse(JSON.stringify($scope.Entity)));
                                    $scope.QuestionsCount = $scope.Questions.length - 1;
                                }

                            }).error(function (result) {

                            });
                        }
                    },
                    cancel: function () {
                        //close
                    },
                },

                onContentReady: function () {
                    // bind to events
                    var jc = this;
                    this.$content.find('form').on('submit', function (e) {
                        // if the user submits the form by pressing enter in the field.
                        e.preventDefault();
                        jc.$$formSubmit.trigger('click'); // reference the button and click it
                    });
                }
            });


        }



        $scope.Search = function () {

            if (($scope.SearchCondition.mobileno == null || $scope.SearchCondition.mobileno == '') &&
                ($scope.SearchCondition.loyeltyno == null || $scope.SearchCondition.loyeltyno == '') &&
                ($scope.SearchCondition.name == null || $scope.SearchCondition.name == '') &&
                ($scope.SearchCondition.religionid == null || $scope.SearchCondition.religionid == '')) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please enter mobile no or name.");
                return;
            }

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "QuestionService/Search",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length == 20000)
                    $rootScope.Toaster.Info("Please refine your search criteria to get more appropriate records.");
                $scope.totalbillvalue = 0;
                $scope.totalreward = 0;
                $scope.SearchResult = result;
                $scope.QuestionConfig();
                if (result != null && result.length == 1) {
                    $scope.ViewResult = result[0].Questionbill;
                    $.each($scope.ViewResult, function (k, v) {
                        $scope.totalbillvalue += v.billvalue;
                        $scope.totalreward += (v.loyelty - v.adjusted);
                    });
                }
                else
                    $scope.ViewResult = [];
            }).error(function (result) {
            });

        }



        $scope.QuestionConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddReferenceColumn('Questiontype', 'Type', 100, $scope.QuestionType);
            igrid.AddSimpleColumn('name', 'Name', 220);
            igrid.AddSimpleColumn('mobileno', 'Mobile Number', 150);
            igrid.AddSimpleColumn('creditlimit', 'Credit Limit ', 100);
            igrid.AddSimpleColumn('loyeltyno', 'Card No ', 100);
            igrid.AddSimpleColumn('loyelty', 'Loyalty', 90);
            igrid.AddSimpleColumn('cityname', 'City', 100);
            igrid.AddSimpleColumn('area', 'Area', 100);
            igrid.AddSimpleColumn('address', 'Address', 150);

            var commands = "";
            if ($rootScope.UserInfo.p_QuestionMASTER.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_QuestionMASTER.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            if ($rootScope.UserInfo.p_QuestionMASTER.modify || $rootScope.UserInfo.p_QuestionMASTER.delete) {
                igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.ManageSearchItem('Edit', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.ManageSearchItem('Delete', args.item);
                    });
                });
            }


            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);

        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = ''
            if ($scope.SearchCondition.companyid != null && $scope.SearchCondition.companyid > 0)
                title = $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company);


            if (tableid == 'Questionreport')
                subtitle = 'Question DETAILS';


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

        $scope.ChangeStatus = function (item, callback) {

            var request = {
                "Item": item,
                "securitytoken": "String content"
            }

            $http({
                method: 'POST',
                url: "QuestionService/Update",
                data: request
            }).success(function (result) {
                if (result) {
                    callback(result);
                }
            }).error(function (result) {
            });

        }

        $scope.ManageStatus = function (type, item) {
            if (type == 'edit') {
                $scope.Question = item;
                $scope.LastAnswerList = [];
            }
            if (type == 'enable') {
                $scope.ChangeStatus({ 'type': type, question:item}, function (result) {
                    item.enable = result;
                });
            }
            if (type == 'disable') {
                $scope.ChangeStatus({ 'type': type, question: item }, function (result) {
                    item.enable = !result;
                });
            }
            if (type == 'movedown') {
                var uitem = { 'selected': item, 'type': type };
                uitem.next = $scope.Questions[$scope.Questions.indexOf(item) + 1];

                $scope.ChangeStatus(uitem, function (result) {
                    if (result) {
                        var selectedindex = uitem.selected.index;
                        uitem.selected.index = uitem.next.index;
                        uitem.next.index = selectedindex;
                        $scope.Questions.splice($scope.Questions.indexOf(uitem.selected), 1);
                        $scope.Questions.splice($scope.Questions.indexOf(uitem.next) + 1, 0, uitem.selected);
                    }
                });
            }
            if (type == 'moveup') {
                var uitem = { 'selected': item, 'type': type };
                uitem.previous = $scope.Questions[$scope.Questions.indexOf(item) - 1];

                $scope.ChangeStatus(uitem, function (result) {
                    if (result) {
                        var selectedindex = uitem.selected.index;
                        uitem.selected.index = uitem.previous.index;
                        uitem.previous.index = selectedindex;
                        $scope.Questions.splice($scope.Questions.indexOf(uitem.selected), 1);
                        $scope.Questions.splice($scope.Questions.indexOf(uitem.previous) - 1, 0, uitem.selected);
                    }
                });
            }
        }

        $timeout(function () {
            var available = (typeof ($rootScope.windowSize) != 'undefined' && typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
            if (available > 0) {
                $scope.panelheight.height = '' + (available - 100) + 'px';
            }
            //$scope.setFocusfirst();
        }, 100);


    }]);