
//var app = angular.module('InfinityLab');

//app.controller('ComponentOutwardsController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', 'cntxService', '$timeout', 'Excel',
//    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, cntxService, $timeout, Excel) {
//        $rootScope.AccordionName = $location.path();
//        $scope.Navigation = $rootScope.GetNavigation();
//        $scope.SearchConditionItems = [{ 'id': 1, 'name': 'Brand' }, { 'id': 2, 'name': 'Product' }, { 'id': 3, 'name': 'Barcoded On' }, { 'id': 4, 'name': 'Pending' }
//            , { 'id': 6, 'name': 'LREntry' }, { 'id': 8, 'name': 'From File' }];

//        $scope.intercompanytransfer = false || $rootScope.UserInfo.appsettings.system_interstatetransfer;

//        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
//            $scope.Employee = results;
//        });
//        $scope.ShowSearch = false;
//        $scope.SearchResults = [];
//        $scope.SearchOutwardResult = [];
//        $scope.SearchRef = restService.SearchReference;

//        $scope.CompanyList = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
//            $scope.CompanyList = results;
//        });

//        $scope.BrandList = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
//            $scope.BrandList = results;
//        });

//        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
//            $scope.ProductList = results;
//        });

//        $scope.Location = restService.Get('ReferencelistService/FULL-LOCATION', true).then(function (results) {
//            $scope.Location = results;
//        });

//        $scope.WHLocation = restService.Get('ReferencelistService/WHLOCATION', true).then(function (results) {
//            $scope.WHLocation = results;
//        });

//        $scope.RetailLocation = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
//            $scope.RetailLocation = [];
//            $.each(results, function (k, v) {
//                if (v.additionalinfo == null || !v.additionalinfo.iswarehouse) {
//                    $scope.RetailLocation.push(v);
//                }
//            });
//        });

//        $scope.SupplierList = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
//            $scope.SupplierList = results;
//        });

//        $scope.LocalContext = cntxService.CreatePageContext('outward');
//        $scope.TotalAccepted = 0;
//        $scope.TotalCounts = 0;
//        $scope.SearchCounts = 0;
//        $scope.Entity = {
//            "id": 0,
//            "items_json": null,
//            "Items": [],
//            "selectAll": false,
//            "code": null,
//            "fromcompanyid": 0,
//            "fromlocationid": 0,
//            "tolocationid": 0,
//            "packedby": 0,
//            "receivedby": 0,
//            "packedon": null,
//            "notes": null,
//            "approvedby": 0,
//            "isactive": true,
//            "createdon": "",
//            "modifiedon": "",
//            "createdby": 0,
//            "modifiedby": 0,
//            "status": 0,
//            "version": 1,
//            "cansave": true,
//            "candelete": false,
//            "salablegoods": [],
//            "checkList": true,
//            tocompanyid: ""
//        };
//        $scope.OutWards = (JSON.parse(JSON.stringify($scope.Entity)));

//        $scope.PrintList = [];
//        $scope.ShowPrint = true;
//        $scope.totalqty = 0;

//        $scope.PushItems = function () {
//            var removeitem = [];
//            $.each($scope.SearchResults, function (key, value) {
//                if (value.selected) {

//                    var existingitem = $scope.OutWards.items.find(function (item) {
//                        return item.stockid == value.stockid;
//                    })

//                    if (typeof (existingitem) == 'undefined' || existingitem == null) {
//                        value.iqty = value.qty;
//                        $scope.OutWards.items.push(value);
//                    }
//                    removeitem.push(value);
//                }
//            });


//            $scope.OutWards.packedon = new Date();


//            $scope.TotalCounts = 0;
//            $.each($scope.OutWards.items, function (k, v) {
//                $scope.TotalCounts += (v.ismeter ? 1 : v.iqty);
//            });


//            $.each(removeitem, function (k, v) {
//                var index = $scope.SearchResults.indexOf(v);
//                $scope.SearchResults.splice(index, 1);
//                v.selected = false;
//            });

//            $scope.SearchCounts = 0;
//            $.each($scope.SearchResults, function (k, v) {
//                $scope.SearchCounts += (v.ismeter ? 1 : v.qty);;
//            });

//            $scope.LocalContext.Post('$scope.OutWards.items', $scope.OutWards.items, false);
//        };

//        $scope.fileuploadedcomplete = function (value) {
//            var barcodelist = [];
//            $.each(value.rows, function (k, v) {
//                barcodelist.push(v.values[0]);
//            });
//            $scope.SearchStock(true, barcodelist.join(','));
//        };


//        $scope.PopItems = function () {
//            var removeitem = [];
//            $.each($scope.OutWards.items, function (key, value) {
//                if (value.selected) {
//                    $scope.SearchResults.push(value);
//                    removeitem.push(value);
//                }
//            });

//            $.each(removeitem, function (k, v) {
//                $scope.OutWards.items.splice($scope.OutWards.items.indexOf(v), 1);
//                v.selected = false;
//            });

//            $scope.TotalCounts = 0;
//            $.each($scope.OutWards.items, function (k, v) {
//                $scope.TotalCounts += (v.ismeter ? 1 : v.iqty);;
//            });

//            $scope.SearchCounts = 0;
//            $.each($scope.SearchResults, function (k, v) {
//                $scope.SearchCounts += (v.ismeter ? 1 : v.qty);;
//            });
//        };

//        $scope.SearchOutwords = {
//            'fromwarehouseoutward': true, 'todate': null, 'fromdate': null
//        };

//        $scope.SearchCondition = {
//            id: 0,
//            date: null,
//            addedfrom: null,
//            addedto: null,
//            productid: null,
//            brandid: null,
//            barcode: null,
//            fromcompanyid: null,
//            searchnewbarcodes: false,
//            searchtype: 4,
//            supplierid: [],
//            suppliername: '',
//            lrentryno: null,
//            selected: false
//        }

//        $scope.LastSaved = function () {
//            var value = $scope.LocalContext.Get('$scope.OutWards.items', false);
//            if (typeof (value) != 'undefined' && value == null) {
//                $rootScope.ShowAlert('No data found', 'No-Data');
//                return;
//            }
//            $scope.OutWards.items = value;
//            $scope.TotalCounts = 0;
//            $.each($scope.OutWards.items, function (k, v) {
//                $scope.TotalCounts += (v.ismeter ? 1 : v.iqty);;
//            });

//        };

//        $scope.currentcompanyid = 0;
//        $scope.SearchStock = function (barcoded, barcodelist) {

//            if ($scope.Appsetting_Company &&
//                (typeof ($scope.OutWards.fromcompanyid) == 'undefined' || $scope.OutWards.fromcompanyid <= 0)) {
//                $rootScope.Toaster.Info(" Please select company");
//                return;
//            }

//            if ($scope.currentcompanyid == 0) {
//                $scope.currentcompanyid = $scope.OutWards.fromcompanyid;
//            }
//            else if ($scope.currentcompanyid != $scope.OutWards.fromcompanyid) {
//                $scope.currentcompanyid = $scope.OutWards.fromcompanyid;
//                $scope.SearchResults = [];
//                $scope.OutWards.items = [];
//            }


//            var request = {
//                "Item": $scope.SearchCondition,
//                "securitytoken": "String content"
//            }

//            if (barcoded) {
//                request = {
//                    "Item": { 'barcode': barcodelist, 'companyid': $scope.OutWards.fromcompanyid, 'searchtype': 5 },
//                    "securitytoken": "String content"
//                }
//            }
//            else {
//                $scope.SearchCondition.companyid = $scope.OutWards.fromcompanyid;
//            }

//            var srequest = JSON.stringify(request);

//            $http({
//                method: 'POST',
//                url: "BundlesService/SearchStock",
//                data: srequest
//            }).success(function (result) {

//                if (angular.isArray(result)) {

//                    $scope.SearchResults = [];

//                    $.each(result, function (k, v) {

//                        if (barcoded) {
//                            var itm = $scope.OutWards.items.find(function (itm) {
//                                return v.stockid == itm.stockid;
//                            });
//                            if (typeof (itm) == 'undefined' || itm == null) {
//                                //$scope.OutWards.items.push(v);
//                                $scope.OutWards.items.splice(0, 0, v);
//                                $scope.SearchCondition.directbarcode = "";
//                                if (!v.ismeter && $scope.OutWards.multiscan) {
//                                    v.iqty = 1;
//                                }

//                                $scope.LocalContext.Post('$scope.OutWards.items', $scope.OutWards.items, false);
//                            }
//                            else {
//                                if (!v.ismeter && $scope.OutWards.multiscan) {
//                                    itm.iqty += 1;
//                                }
//                                else {
//                                    itm.qty = v.qty;
//                                    itm.iqty = v.iqty;
//                                }
//                                $scope.SearchCondition.directbarcode = "";
//                                $scope.LocalContext.Post('$scope.OutWards.items', $scope.OutWards.items, false);
//                            }
//                        }
//                        else {
//                            var itm = $scope.SearchResults.find(function (itm) {
//                                return v.stockid == itm.stockid;
//                            });
//                            if (typeof (itm) == 'undefined' || itm == null)
//                                $scope.SearchResults.push(v);

//                            if ($scope.SearchCondition.searchtype == 5) {
//                                $scope.SearchCondition.barcode = "";
//                            }
//                        }

//                    });
//                }


//                $scope.SearchCounts = 0;
//                $scope.TotalCounts = 0;
//                $.each($scope.SearchResults, function (k, v) {
//                    $scope.SearchCounts += (v.ismeter ? 1 : v.qty);
//                });
//                $scope.CalculateTotal();

//            }).error(function (result) {
//            });
//        }

//        $scope.CalculateTotal = function () {
//            $scope.TotalCounts = 0;
//            $.each($scope.OutWards.items, function (k, v) {
//                $scope.TotalCounts += (v.ismeter ? 1 : v.iqty);
//            });
//        }

//        $scope.selectAllItems = function (items, mode) {

//            $.each(items, function (key, value) {
//                value.selected = mode;
//            });

//            /*if ($scope.OutWards.selectAll) {
//                $.each($scope.SearchResults, function (key, value) {
//                    $scope.SearchResults[key].selected = true;
//                });
//            }
//                else {
//                    $.each($scope.SearchResults, function (key, value) {
//                        $scope.SearchResults[key].selected = false;
//                    });
//            }*/

//        }

//        $scope.ErrorStyle = "success";
//        $scope.ShowSearch = false;
//        $scope.SearchResult = [];

//        $scope.SwitchPanel = function () {
//            $scope.ShowSearch = !$scope.ShowSearch;
//            $scope.OutWards = (JSON.parse(JSON.stringify($scope.Entity)));
//        }

//        $scope.ErrorMessage = "";

//        $scope.Clear = function () {
//            $scope.OutWards = (JSON.parse(JSON.stringify($scope.Entity)));
//            $scope.SearchResults = [];
//            $scope.OutWards.items = [];
//            $scope.TotalAccepted = 0;
//            $scope.TotalCounts = 0;
//        };

//        $scope.Save = function () {
//            var iserror = false;
//            var wrongbarcode = '';
//            $scope.TotalCounts = 0;
//            $.each($scope.OutWards.items, function (k, v) {
//                if (v.qty < v.iqty || v.iqty <= 0) {
//                    wrongbarcode = v.barcode;
//                    iserror = true;
//                }
//            });

//            if (iserror) {
//                $rootScope.ShowAlert('No data found', 'Wrong Out qty for - ' + wrongbarcode);
//                return;
//            }

//            if ($scope.OutWards.packedon == null) {
//                $scope['setFocusfirst']();
//                $rootScope.Toaster.Info(" Please select date");
//                return;
//            }
//            if ($scope.OutWards.fromcompanyid == null || $scope.OutWards.fromcompanyid == 0) {
//                $scope['setFocusfromcompanyid']();
//                $rootScope.Toaster.Info("Please select company");
//                return;
//            }

//            if ($scope.OutWards.packedby == null || $scope.OutWards.packedby == 0) {
//                $scope['setFocuspackedby']();
//                $rootScope.Toaster.Info("Please select packedby");
//                return;
//            }
//            if ($scope.OutWards.fromlocationid == null || $scope.OutWards.fromlocationid == 0) {
//                $scope['setFocusfromlocationid']();
//                $rootScope.Toaster.Info("Please select fromlocation");
//                return;
//            }

//            if ($scope.OutWards.tolocationid == null || $scope.OutWards.tolocationid == 0) {
//                $scope['setFocustolocationid']();
//                $rootScope.Toaster.Info("Please select tolocation");
//                return;
//            }

//            $scope.OutWards.inward2warehouse = true;

//            request = {
//                "Item": $scope.OutWards,
//                "securitytoken": "String content"
//            }

//            var srequest = JSON.stringify(request);
//            if ($scope.SaveRequest) return;
//            $scope.SaveRequest = true;
//            $http({
//                method: 'POST',
//                url: "BundlesService/Outward",
//                data: srequest
//            }).success(function (result) {
//                $scope.SaveRequest = false;
//                if (result != null && result.id > 0) {
//                    $scope.LocalContext.Post('$scope.OutWards.items', null, false);
//                    $rootScope.Toaster.Info("OutWards Saved Successfully.!");
//                    $scope.OutWards.code = result.code;
//                }
//                else {
//                    $rootScope.Toaster.Error(result.errormessage);
//                }

//            }).error(function (result) {
//                $scope.SaveRequest = false;
//            });
//        }

//        $scope.Search = function () {
//            request = {
//                "Item": $scope.SearchOutwords,
//                "securitytoken": "String content"
//            }

//            var srequest = JSON.stringify(request);

//            $http({
//                method: 'POST',
//                url: "BundlesService/Search",
//                data: srequest
//            }).success(function (result) {

//                $scope.SearchOutwardResult = result;

//                $.each(result, function (k, v) {

//                    v.cansave = false;
//                    v.candelete = $scope.OutWards.status == 0
//                    v.statusdisplay = '';
//                    v.canreject = false;
//                    switch (v.status) {
//                        case 0:
//                            v.statusdisplay = 'New Bundle';
//                            break;
//                        case 1:
//                            v.statusdisplay = 'Inwarded Bundle';
//                            break;
//                        case 2:
//                            v.statusdisplay = 'Inwarded Bundle with changes';
//                            break;
//                        case 9:
//                            v.statusdisplay = 'Rejected Bundle';
//                            break;
//                    }

//                })



//            }).error(function (result) {
//            });
//        }


//        $scope.SearchByCode = function () {
//            request = {
//                "Item": {
//                    'code': $scope.OutWards.code,
//                    'fromwarehouseoutward': true
//                },
//                "securitytoken": "String content"
//            }

//            var srequest = JSON.stringify(request);

//            $http({
//                method: 'POST',
//                url: "BundlesService/Search",
//                data: srequest
//            }).success(function (result) {
//                if ($.isArray(result) && result.length > 0) {
//                    $scope.LoadOutward(result[0]);
//                }
//                else {
//                    $scope.OutWards.statusdisplay = 'Not Found!';
//                }
//            }).error(function (result) {
//            });
//        }

//        $scope.LoadOutward = function (item) {
//            $scope.OutWards = item;
//            $scope.OutWards.cansave = false;
//            $scope.OutWards.candelete = $scope.OutWards.status == 0
//            $scope.OutWards.statusdisplay = '';
//            $scope.OutWards.canreject = false;
//            switch ($scope.OutWards.status) {
//                case 0:
//                    $scope.OutWards.statusdisplay = 'New Bundle';
//                    break;
//                case 1:
//                    $scope.OutWards.statusdisplay = 'Inwarded Bundle';
//                    break;
//                case 2:
//                    $scope.OutWards.statusdisplay = 'Inwarded Bundle with changes';
//                    break;
//                case 9:
//                    $scope.OutWards.statusdisplay = 'Rejected Bundle';
//                    break;
//            }

//            if ($scope.OutWards.status == 9)
//                $scope.OutWards.id = 0;

//            $scope.TotalCounts = 0;
//            $scope.TotalAccepted = 0;
//            $.each($scope.OutWards.items, function (k, v) {
//                $scope.TotalCounts += (v.ismeter ? 1 : v.iqty);
//                if (v.aqty > 0)
//                    $scope.TotalAccepted += (v.ismeter ? 1 : v.aqty);
//            });
//            $scope.Navigation.SetMode('EDIT');
//            $scope.ShowSearch = false;
//        }

//        $scope.ShowSuppliers = function () {
//            var dialog = ngDialog.open({
//                template: 'SupplierSearch',
//                data: [$scope, $scope.SupplierList],
//                width: '600px',
//                controller: ['$scope', '$timeout', function ($scope, $timeout) {
//                    $scope.SupplierList = $scope.ngDialogData[1];
//                    $scope.selectall = true;
//                    $.each($scope.SupplierList, function (k, v) {
//                        v.selected = true;
//                    });

//                    $scope.selectallChanged = function () {
//                        $.each($scope.SupplierList, function (k, v) {
//                            v.selected = $scope.selectall;
//                        });
//                    };

//                    $scope.Selected = function () {
//                        var ret = {
//                            'name': '', supplierid: []
//                        };
//                        $.each($scope.SupplierList, function (k, v) {
//                            if (typeof (v.selected) != 'undefined' && v.selected) {
//                                ret.name += (ret.name.length > 0 ? ',' : '') + v.name;
//                                ret.supplierid.push(v.id);
//                            }
//                        });
//                        $scope.closeThisDialog(ret);
//                    };
//                }],
//                className: 'ngdialog-theme-plain'
//            });
//            dialog.closePromise.then(function (data) {
//                {
//                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
//                        $scope.SearchCondition.suppliername = data.value.name;
//                        $scope.SearchCondition.supplierid = data.value.supplierid;
//                    }
//                }
//            });
//        }

//        $scope.ManageSearchItem = function (action, item) {

//            if (action == 'Delete') {
//                request = {
//                    "Item": item,
//                    "securitytoken": "String content"
//                }
//                var srequest = JSON.stringify(request);
//                $http({
//                    method: 'POST',
//                    url: "BundlesService/Delete",
//                    data: srequest
//                }).success(function (result) {

//                    if (result) {
//                        $rootScope.Toaster.Info("Successfully Removed");
//                    }
//                    else {
//                        $rootScope.Toaster.Info("Some of the items already sold, can't reject");
//                    }

//                }).error(function (result) {
//                });
//            } else if (action == "Edit") {
//                $scope.LoadOutward(item);
//            }
//        }


//        $scope.Delete = function () {
//            request = {
//                "Item": $scope.OutWards,
//                "securitytoken": "String content"
//            }
//            var srequest = JSON.stringify(request);
//            $http({
//                method: 'POST',
//                url: "BundlesService/Delete",
//                data: srequest
//            }).success(function (result) {

//                if (result) {
//                    $scope.statusdisplay = "Successfully Removed";
//                    $scope.ErrorStyle = "success";
//                    $scope.OutWards = (JSON.parse(JSON.stringify($scope.Entity)));
//                    $scope.TotalCounts = 0;
//                    $scope.SearchCounts = 0;
//                    $scope.SearchResults = [];
//                }
//                else {
//                    $scope.statusdisplay = "Some of the items already sold, can't reject";
//                    $scope.ErrorStyle = "danger";
//                }

//            }).error(function (result) {
//            });

//        }
//        $scope.Reject = function () {

//            if ($scope.SaveRequest) return;
//            $scope.SaveRequest = true;
//            request = {
//                "Item": $scope.OutWards,
//                "securitytoken": "String content"
//            }
//            var srequest = JSON.stringify(request);
//            $http({
//                method: 'POST',
//                url: "BundlesService/Reject",
//                data: srequest
//            }).success(function (result) {
//                $scope.SaveRequest = false;
//                if (result) {
//                    $scope.ErrorMessage = "Successfully Removed";
//                    $scope.ErrorStyle = "success";
//                }
//                else {
//                    $scope.ErrorMessage = "Some of the items already sold, can't reject";
//                    $scope.ErrorStyle = "danger";
//                }

//            }).error(function (result) {
//                $scope.SaveRequest = false;
//            });
//        }

//        $scope.Clear();

//        var pagesize = 20;

//        $scope.PrintPage = function () {
//            $timeout(function () {

//                var otitle = angular.element('#apptitle').context.title;
//                var titlele = angular.element('#apptitle').context.title = "OUTWARDS SLIP";

//                var docHead = document.head.outerHTML;

//                var table = $('#printdiv');
//                var ctable = table.clone();

//                var width = 800, height = 900;
//                var printContents = ctable[0].outerHTML;
//                var winAttr = "location=yes, statusbar=no, menubar=no, titlebar=no, toolbar=no,dependent=no, width=" + width + ", height=" + height + ", resizable=yes, screenX=200, screenY=200, personalbar=no, scrollbars=yes";

//                var newWin = window.open("", "_blank", winAttr);
//                var writeDoc = newWin.document;
//                writeDoc.open();
//                writeDoc.write('<!doctype html><html>' + docHead + '<body onLoad="window.print()">' + printContents + '</body></html>');
//                writeDoc.close();
//                newWin.focus();

//                angular.element('#apptitle').context.title = otitle;
//            }, 100);
//        }

//        $scope.exporttoExcel = function (tableid, title) {
//            $scope.exportHref = Excel.tableToExcel(tableid, title, 1, 'Outward');
//            if ($scope.exportHref != null) {
//                $timeout(function () {
//                    var link = document.createElement('a');
//                    link.download = "outward.xls";
//                    link.href = $scope.exportHref;
//                    document.body.appendChild(link);
//                    link.click();
//                    link.remove();
//                }, 100);
//            }
//        }

//        $scope.PrintPDF = function (item) {
//            $timeout(function () {
//                var link = document.createElement('a');
//                link.download = "debitnote.pdf";
//                link.href = 'BundlesService/PrintPDF/' + $scope.OutWards.id;
//                document.body.appendChild(link);
//                link.click();
//                $timeout(function () {
//                    link.remove();
//                }, 100);
//            }, 10);
//        }
//        $scope.PrintDocumentList = [];
//        $scope.PrintOW = function () {
//            $scope.totalqty = 0.0;

//            request = {
//                "Item": { 'id': $scope.OutWards.id },
//                "securitytoken": "String content"
//            }

//            var srequest = JSON.stringify(request);

//            $http({
//                method: 'POST',
//                url: "BundlesService/Print",
//                data: srequest
//            }).success(function (result) {
//                $scope.PrintDocument = result;
//                $scope.PrintDocumentList = [];
//                var items = [];

//                $.each(result.items, function (k, v) {
//                    var exist = items.find(function (item) {
//                        return item.productid == v.productid;
//                    });
//                    if (exist == null) {
//                        var item = (JSON.parse(JSON.stringify(v)));
//                        items.push(item);
//                    }
//                    else {
//                        exist.iqty += v.iqty;
//                    }
//                });

//                $.each(items, function (k, v) {
//                    $scope.PrintDocumentList.push(v);
//                });

//                $.each($scope.PrintDocumentList, function (k, v) {
//                    $scope.totalqty += v.iqty;
//                });
//                $scope.PrintPage();

//            }).error(function (result) {
//            });
//        }

        

//        $scope.Appsetting_Company = false;
//        $scope.LoadApplicationSetting = function () {
//            if (typeof ($rootScope.UserInfo) != 'undefined' &&
//                typeof ($rootScope.UserInfo.appsettings) != 'undefined') {
//                $scope.Appsetting_Company = true;
//            }
//            else {
//                $timeout(function () {
//                    $scope.LoadApplicationSetting();
//                }, 50);
//            }
//        }


//        $timeout(function () {
//            $scope.LoadApplicationSetting();
//            $scope.setFocusfirst();
//        }, 10);
//        $scope.CreateNew = function () {
//            $scope.OutWards.packedon = new Date();
//        }();
//    }]);
