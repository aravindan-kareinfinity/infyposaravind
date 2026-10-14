
var app = angular.module('InfinityLab');

app.controller('CustomerDivertController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;

        $scope.DivertStatus = [{ 'id': 1, 'name': 'PENDING' }, { 'id': 2, 'name': 'REVIEW' }, { 'id': 3, 'name': 'COMPLETED' }, { 'id': 4, 'name': 'CLOSED' }];
        $scope.CustomerType = [{ 'id': 1, 'name': 'GIRLS' }, { 'id': 2, 'name': 'BOYS' }, { 'id': 3, 'name': 'MENS' }, { 'id': 4, 'name': 'LADIES' }, { 'id': 5, 'name': 'FAMILY' }];

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.Sections = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Sections = results;
        });

        $scope.Entity = {
            "id": 0,
            "diverton": new Date(),
            "name": "",
            "ccinchargeid": 0,
            "sectionid": 0,
            "customertypeid": 0,
            "reason": "",
            "customername": "",
            "mobileno": "",
            "sectioninchargeid": 0,
            "sectionfeedback": "",
            "actiontakenbyid": 0,
            "actionstatus": 1,
            "actioncomments": "",
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "0001-01-01T00:00:00",
            "modifiedon": "0001-01-01T00:00:00"
        };

        $scope.Customerdivert = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Customerdivert = (JSON.parse(JSON.stringify($scope.Entity)));
        }
        $scope.ErrorMessage = "";

        $scope.SearchCondition = {
            fromdate: '',
            todate: '',
            sectionid: 0,
            ccinchargeid: 0
        }

        $scope.New = function () {
            $scope.Customerdivert = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ErrorMessage = "";
        }

        $scope.Save = function () {

            request = {
                "Item": $scope.Customerdivert,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerdivertService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {

                    $rootScope.Toaster.Info("Customer Divert Information Saved Successfully.!");
                    $scope.Customerdivert = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);

                }

            }).error(function (result) {
            });

        }


        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {
                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ' + item.name + "?",
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
                                    "Item": item,
                                    "SecurityToken": "String content"
                                }
                                // var srequest = JSON.stringify(request);
                                $http({
                                    method: 'POST',
                                    url: "CustomerdivertService/Delete",
                                    data: request
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
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
                        jc.$$formSubmit.trigger('focus');
                        this.$content.find('form').on('submit', function (e) {
                            // if the user submits the form by pressing enter in the field.
                            e.preventDefault();
                            jc.$$formSubmit.trigger('click'); // reference the button and click it
                        });
                    }
                });


            }
            else if (action == "Edit") {
                $scope.Navigation.SetMode('EDIT');
                $scope.ShowSearch = false;
                $scope.Customerdivert = item;
            }
        }



        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            //    var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerdivertService/Search",
                data: request
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.CustomerdivertConfig();
            }).error(function (result) {
            });
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'CUSTOMER DIVERT REPORT'
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


        $scope.CustomerdivertConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddFormatedColumn('diverton', 'Date', 80, 'dd-MM-yyyy hh:mm a');
            igrid.AddReferenceColumn('ccinchargeid', 'Incharge', 120, $scope.Employee);
            igrid.AddReferenceColumn('sectionid', 'Section', 120, $scope.Sections);
            igrid.AddReferenceColumn('customertypeid', 'CustomerType', 120, $scope.CustomerType);
            igrid.AddSimpleColumn('customername', 'Customer Name', 120);
            igrid.AddSimpleColumn('mobileno', 'Mobileno', 80);
            igrid.AddSimpleColumn('reason', 'Reason', 120);
            igrid.AddReferenceColumn('sectioninchargeid', 'Section Incharge', 120, $scope.Employee);
            igrid.AddSimpleColumn('sectionfeedback', 'Section Feedback', 140);
            igrid.AddSimpleColumn('actioncomments', 'Action', 100);
            igrid.AddReferenceColumn('actionstatus', 'Status', 80, $scope.DivertStatus);

            var commands = "";
            if ($rootScope.UserInfo.p_CUSTOMERDIVERT.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_CUSTOMERDIVERT.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            if ($rootScope.UserInfo.p_CUSTOMERDIVERT.modify || $rootScope.UserInfo.p_CUSTOMERDIVERT.delete) {
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

        $timeout(function () {
            $scope.setFocusfirst();
        });
}]);



