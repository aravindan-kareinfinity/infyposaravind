var app = angular.module('InfinityLab');
app.controller('PermissionSlipController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout','$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, $filter) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.States = restService.Get('ReferencelistService/STATE', true).then(function (results) {
            $scope.States = results;
        });


        $scope.Company = restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.Mode = [
            { 'id': 1, 'name': 'Permission Slip' },
            { 'id': 2, 'name': 'Leave Slip' },
            { 'id': 3, 'name': 'On Duty Slip' }];

        $scope.Entity = {
            "id": "",
            "requestedon": new Date(),
            "fromleavedate": new Date(),
            "toleavedate": new Date(),
            "totime": "",
            reason: "",
            "fromtime": "",
            "employeeid": 0,
            "approvedby": 0,
            ishalfday: "",
            deductsalary: "",
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "",
            "modifiedon": "",
            mode: ""
        };

        $scope.PermissionSlip = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.PermissionSlip = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ErrorMessage = "";



        $scope.SearchCondition = {
            fromdate: "",
            todate: "",
            employeeid: ""
        }

        $scope.New = function () {
            $scope.PermissionSlip = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ErrorMessage = "";
        }

        $scope.ValidTime = function (value) {
            if (typeof (value) == 'undefined' || value == null) return false;
            var parts = value.split(' ');
            if (parts.length != 2) return false;
            var timeparts = parts[0].split(':');
            if (parts.length != 2) return false;
            if (parts[1] != 'am' && parts[1] != 'pm') return false;
            if (parseInt(timeparts[0]) > 12 || parseInt(timeparts[1]) > 59) return false;
            if (parseInt(timeparts[0]) < 0 || parseInt(timeparts[1]) < 0) return false;
            return true;
        }
        $scope.Save = function () {

            if ($scope.PermissionSlip.mode == 1) {
                if (!$scope.ValidTime($scope.PermissionSlip.fromtime)) {
                    $rootScope.Toaster.Error('From time invalid');
                    return;
                }
                if (!$scope.ValidTime($scope.PermissionSlip.totime)) {
                    $rootScope.Toaster.Error('To time invalid');
                    return;
                }
            }

            request = {
                "Item": $scope.PermissionSlip,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PermissionSlipService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $scope.SearchResult = result;

                    $rootScope.Toaster.Info("PermissionSlip Saved Successfully.!");
                }
                else {

                    $rootScope.Toaster.Error(result.ErrorMessage);

                }

            }).error(function (result) {
            });

        }



        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PermissionSlipService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.PermissionSlipConfig();
            }).error(function (result) {
            });

        }

        $scope.PermissionSlipConfig = function () {

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchResult);

            igrid.AddReferenceColumn('employeeid', 'Employee Name', 150, $scope.Employee);
            igrid.AddReferenceColumn('mode', 'Type', 100, $scope.Mode);

            igrid.AddFormatedColumn('requestedon', 'Requested Date', 100, 'dd-MM-yyyy');

            igrid.AddCustomColumn('fromleavedate', 'From', 70, function (item, col) {
                if (item.mode == 1)
                    return $filter('date')(item.fromleavedate, 'dd-MM-yyyy') + ' ' + $filter('date')(item.fromtime, 'hh:mm a');
                return $filter('date')(item.fromleavedate, 'dd-MM-yyyy');
            });

            igrid.AddCustomColumn('toleavedate', 'To', 70, function (item, col) {
                if (item.mode == 1)
                    return $filter('date')(item.toleavedate, 'dd-MM-yyyy') + ' ' + $filter('date')(item.totime, 'hh:mm a');
                return $filter('date')(item.toleavedate, 'dd-MM-yyyy');
            });

            //igrid.AddFormatedColumn('fromleavedate', 'From Date', 100, 'dd-MM-yyyy');
            //igrid.AddFormatedColumn('fromtime', 'From Time', 100, 'hh:mm a');

            //igrid.AddFormatedColumn('toleavedate', 'To Date', 100, 'dd-MM-yyyy');
            //igrid.AddFormatedColumn('totime', 'To Time', 100, 'hh:mm a');

            igrid.AddSimpleColumn('reason', 'Reason', 100);
            igrid.AddReferenceColumn('approvedby', 'Approved By', 150, $scope.Employee);

            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Edit'/>"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Print'/>"
            igrid.AddButtonColumn('action', 'ACTION', 180, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Edit')
                        $scope.ManageSearchItem('Edit', args.item);
                    else if ($(e.target).val() == 'Delete')
                        $scope.ManageSearchItem('Delete', args.item);
                    else if ($(e.target).val() == 'Print')
                        $scope.ManageSearchItem('Print', args.item);
                });
            });

            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);

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
                                var srequest = JSON.stringify(request);
                                $http({
                                    method: 'POST',
                                    url: "PermissionSlipService/Delete",
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
                $scope.PermissionSlip = item;
            } else if (action == "Print") {
                $rootScope.Download('PermissionSlipService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + item.id, 'PermissionSlip.pdf', 'application/pdf');
                return;
                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = "application.pdf";
                    link.href = 'PermissionSlipService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + item.id;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
        }

        $scope.printDiv = function () {

            request = {
                "Item": { 'id': $scope.SearchResult.id },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PermissionSlipService/Print",
                data: srequest
            }).success(function (result) {

                $scope.PrintDocument = result;

                $timeout(function () {

                    var otitle = angular.element('#apptitle').context.title;
                    var titlele = angular.element('#apptitle').context.title = "SLIP PRINT";

                    var docHead = document.head.outerHTML;

                    var table = $('#printdiv');
                    var ctable = table.clone();


                    var width = 800, height = 900;
                    var printContents = ctable[0].outerHTML;
                    var winAttr = "location=yes, statusbar=no, menubar=no, titlebar=no, toolbar=no,dependent=no, width=" + width + ", height=" + height + ", resizable=yes, screenX=200, screenY=200, personalbar=no, scrollbars=yes";

                    var newWin = window.open("", "_blank", winAttr);
                    var writeDoc = newWin.document;
                    writeDoc.open();
                    writeDoc.write('<!doctype html><html>' + docHead + '<body onLoad="window.print()">' + printContents + '</body></html>');
                    writeDoc.close();
                    newWin.focus();

                    angular.element('#apptitle').context.title = otitle;
                }, 100);
            }).error(function (result) {
            });

            var titlele = angular.element('#apptitle').context.title = title;

            var docHead = document.head.outerHTML;
            var printContents = document.getElementById(div).outerHTML;
            var winAttr = "location=yes, statusbar=no, menubar=no, titlebar=no, toolbar=no,dependent=no, width=865, height=600, resizable=yes, screenX=200, screenY=200, personalbar=no, scrollbars=yes";

            var newWin = window.open("", "_blank", winAttr);
            var writeDoc = newWin.document;
            writeDoc.open();
            writeDoc.write('<!doctype html><html>' + docHead + '<body onLoad="window.print()">' + printContents + '</body></html>');
            writeDoc.close();
            newWin.focus();
        }

    }]);