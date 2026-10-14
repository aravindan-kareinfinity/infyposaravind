
var app = angular.module('InfinityLab');

app.controller('StoreTransactionController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout','$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, $filter) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.Mode = 'Fine';
        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];
        $scope.Entity = {
            fromdate: datePicker.getDate(new Date()),
            todate: datePicker.getDate(new Date()),
            actiondate: datePicker.getDate(new Date()),
            toactiondate: datePicker.getDate(new Date()),
            fromactiondate: datePicker.getDate(new Date()),
            type: "",
            StoreOpeningAttribute: {
                'opening': '', 'closing': ''
            },
            EBReadingAttribute: {
                'no': '', 'reading': ''
            },
            TokenAttribute: {
                'startno': '', 'endno': ''
            },
            ReferralTokenAttribute: {
                'startno': '', 'endno': '',
                'employeeid': 0, 'agentid': 0,
                'discount':0,'discountpercentage':0
            },
            companyid: "",
            locationid: "",
            isactive: true,
            version: 1
        };
        $scope.Modes = [{ 'id': 'StoreTime', 'name': 'Store Opening/Closing Time' },
            { 'id': 'EBReading', 'name': 'EB Reading' },
            { 'id': 'WalkIn', 'name': 'Walkin Token' },
            { 'id': 'Referral', 'name': 'Referral Token' },];

        $scope.StoreEntry = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorMessage = "";

        $scope.SearchCondition = {
            name: "",
            contactperson: ""
        }

        $scope.Location = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Location = results;
        });

        $scope.Company = restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.Employees = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employees = results;
        });

        $scope.Agents = restService.Get('ReferencelistService/AGENT/4', true).then(function (results) {
            $scope.Agents = results;
        });

        $scope.New = function () {
            $scope.StoreEntry = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.PrintReferralToken = function () {
            if ($scope.StoreEntry.discountpercentage) {
                var discountpercentage = parseFloat($scope.StoreEntry.discountpercentage);
                if (isNaN(discountpercentage)) discountpercentage = 0;
                var accepteddiscount = parseFloat($rootScope.UserInfo.AD);
                if (isNaN(accepteddiscount)) accepteddiscount = 0;
                if (discountpercentage > accepteddiscount) {
                    $rootScope.Toaster.Error("You don't have permssion to print discount % slip");
                    return;
                }
            }

            if ($scope.StoreEntry.discount) {
                var discount = parseFloat($scope.StoreEntry.discount);
                if (isNaN(discount)) discount = 0;
                var accepteddiscount = parseFloat($rootScope.UserInfo.AD);
                if (isNaN(accepteddiscount)) accepteddiscount = 0;
                if (accepteddiscount < 50) {
                    $rootScope.Toaster.Error("You don't have permssion to print discount value slip");
                    return;
                }
            }
            $scope.PrintToken();
        }

        $scope.PrintToken = function () {
            var request = {
                "Item": $scope.StoreEntry,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "StoretransactionService/PrintToken",
                data: srequest
            }).success(function (result) {
                $scope.StoreEntry = result;
                if (result.printdocument != null)
                    $rootScope.Sent2Printer(result.printdocument);
                result.printdocument = null;
                $scope.LoadWalkin(result.type);
            }).error(function (result) {
            });
        }

        $scope.Clear = function () {
            $scope.StoreEntry = (JSON.parse(JSON.stringify($scope.Entity)));
            if ($scope.igrid != null)
                $scope.igrid.destroy();
        }

        $scope.SearchStoreTransaction = function () {
            request = {
                "Item": $scope.StoreEntry,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "StoretransactionService/Search",
                data: srequest
            }).success(function (result) {
                $scope.ShowOpeningResult(result);

            }).error(function (result) {
            });
        }

        $scope.modechanged = function () {
            $scope.StoreEntry.id = 0;
            $scope.StoreEntry.WalkinTokenAttribute = {};
            $scope.StoreEntry.ReferralTokenAttribute = {};
            $scope.walkincount = '';
        }

        $scope.SaveStoreTransaction = function () {

            if ($scope.StoreEntry.type == 'WalkIn' &&
                (typeof ($scope.StoreEntry.id) == 'undefined' ||
                    $scope.StoreEntry.id == 0 ||
                    $scope.StoreEntry.id == null)) {

                $scope.StoreEntry.WalkinTokenAttribute = {
                    'startno': 0, 'endno': 0
                };
                var parts = StoreEntry.tokennumbers.split('-');
                if (parts.length == 2) {
                    $scope.StoreEntry.WalkinTokenAttribute.startno = parseInt(parts[0]);
                    $scope.StoreEntry.WalkinTokenAttribute.endno = parseInt(parts[1]);
                }
                else {
                    $scope.StoreEntry.WalkinTokenAttribute.startno = 1;
                    $scope.StoreEntry.WalkinTokenAttribute.endno = parseInt(StoreEntry.tokennumbers);
                }
            }

            request = {
                "Item": $scope.StoreEntry,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "StoretransactionService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $scope.StoreEntry = result;
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
            });

        }

        $scope.LoadWalkin = function (type) {

            if (new Date($scope.StoreEntry.actiondate).getFullYear < 2000) return;

            request = {
                "Item": {
                    'type': type,
                    'actiondate': $scope.StoreEntry.actiondate,
                    'locationid': $scope.StoreEntry.locationid
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "StoretransactionService/Search",
                data: srequest
            }).success(function (result) {

                if (result != null && result.length > 0) {

                    if ($filter('date')($scope.StoreEntry.actiondate, 'dd-MM-yyyy') ==
                        $filter('date')(new Date(result[0].actiondate), 'dd-MM-yyyy')) {
                        $scope.StoreEntry = result[0];
                        //$scope.StoreEntry.actiondate = $filter('date')(new Date(result[0].actiondate), 'dd-MM-yyyy');
                    }
                }
                else {
                    $scope.StoreEntry.id = 0;
                    $scope.StoreEntry.WalkinTokenAttribute = {};
                    $scope.StoreEntry.ReferralTokenAttribute = {};
                    $scope.walkincount = '';
                }

            }).error(function (result) {
            });
        }

        $scope.WalkInCount = function () {
            var request = {
                "item": {
                    'walkincount': $scope.walkincount.replaceAll('$', ''),
                    'walkintoken': $scope.walkincount,
                    'locationid': $scope.StoreEntry.locationid,
                    'isactive': true
                },
                "securitytoken": "String content"
            };

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerwalkinsService/DirectEntry",
                data: srequest
            }).success(function (result) {
                $scope.LoadWalkin('WalkIn');
                $scope.walkincount = '';
            });
        }

        $scope.ShowOpeningResult = function (SearchResult) {

            $scope.SearchResult = SearchResult;

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 10);
            igrid.AddFormatedColumn('actiondate', 'Date', 100, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('companyid', 'Company', 150, $scope.Company);
            igrid.AddReferenceColumn('locationid', 'Location', 150, $scope.Location);

            if ($scope.StoreEntry.type == 'StoreTime') {
                igrid.AddCustomColumn('opening', 'Opening', 100, function (item, col) {
                    return item.StoreOpeningAttribute != null ? item.StoreOpeningAttribute.opening : '';
                });
                igrid.AddCustomColumn('closing', 'closing', 100, function (item, col) {
                    return item.StoreOpeningAttribute != null ? item.StoreOpeningAttribute.closing : '';
                });
            }
            else if ($scope.StoreEntry.type == 'EBReading') {
                igrid.AddCustomColumn('no', 'No', 100, function (item, col) {
                    return item.EBReadingAttribute != null ? item.EBReadingAttribute.no : '';
                });
                igrid.AddCustomColumn('reading', 'Reading', 100, function (item, col) {
                    return item.EBReadingAttribute != null ? item.EBReadingAttribute.reading : '';
                });
            }
            else if ($scope.StoreEntry.type == 'WalkIn') {
                igrid.AddCustomColumn('startno', 'Start', 100, function (item, col) {
                    return item.WalkinTokenAttribute != null ? item.WalkinTokenAttribute.startno : '';
                });
                igrid.AddCustomColumn('endno', 'End', 100, function (item, col) {
                    return item.WalkinTokenAttribute != null ? item.WalkinTokenAttribute.endno : '';
                });
            }
            else {
                igrid.AddCustomColumn('type', 'Type', 100, function (item, col) {
                    return item.type;
                });
            }

            var commands = "<input type='button' class='btn btn-danger btn-xs' value='Delete'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Edit'/> &nbsp;"
            
            igrid.AddButtonColumn('action', 'ACTION', 175, commands, function (e, args) {
                $scope.$apply(function () {

                    if ($(e.target).val() == 'Edit') {
                        $scope.StoreEntry = args.item;
                        $scope.ShowSearch = false;
                        $scope.Navigation.SetMode('EDIT');
                    }
                    else if ($(e.target).val() == 'Delete')
                        $scope.Delete(args.item);
                });
            });

            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(SearchResult);
        }

        $scope.ShowWalkinSummary = function () {
            request = {
                "Item": { 'id': $scope.StoreEntry.id},
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "StoretransactionService/WalkinSummary",
                data: srequest
            }).success(function (result) {
                $scope.ShowWalkinSummaryGrid(result);
            }).error(function (result) {
            });
        }

        $scope.sgrid = null;
        $scope.wgrid = null;
        $scope.ShowWalkinSummaryGrid = function (results) {

            if ($scope.sgrid != null)
                $scope.sgrid.destroy();

            var igrid = new InfyGrid('gridWalikinContainer', '450px', '100%', true, false, true, 10);
            igrid.AddSimpleColumn('tokenno', 'Token No', 80);
            igrid.AddSimpleColumn('tokenprinted', 'Printed', 70);
            igrid.AddSimpleColumn('tokenissued', 'Issue', 70);
            igrid.AddSimpleColumn('billno', 'Bill No', 100);
            igrid.AddSimpleColumn('billprinted', 'Billed', 70);
            igrid.AddSimpleColumn('customername', 'Customer Name', 100);
            igrid.AddSimpleColumn('mobileno', 'Mobile No', 70);

            igrid.Initialize();
            $scope.sgrid = igrid;
            $scope.sgrid.UpdateData(results.List);

            if ($scope.wgrid != null)
                $scope.wgrid.destroy();

            var wgrid = new InfyGrid('gridSummaryContainer', '450px', '100%', true, false, true, 10);
            wgrid.AddSimpleColumn('hour', 'Time', 60);
            wgrid.AddSimpleColumn('printed', 'Printed', 60).total=true;
            wgrid.AddSimpleColumn('issued', 'Issue', 60).total = true;
            wgrid.AddSimpleColumn('billed', 'Token - Bill', 100).total = true;
            wgrid.AddSimpleColumn('actualbilled', 'Total Bill', 120).total = true;
            wgrid.EnableTotal();
            wgrid.Initialize();
            $scope.wgrid = wgrid;
            $scope.wgrid.UpdateData(results.Hourly);
        }

        $scope.exportExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'WALK-IN STATUS REPORT ON : ' + $filter('date')(new Date(), 'dd-MMM-yyyy');

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

        $scope.Delete = function (item) {

            $.confirm({
                title: 'Delete',
                content: 'Do you want to delete ?',
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
                                url: "StoretransactionService/Delete",
                                data: srequest
                            }).success(function (result) {

                                if (result) {
                                    $scope.ErrorMessage = "Successfully deleted";
                                    $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                                    $scope.igrid.referesh();
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

    }]);