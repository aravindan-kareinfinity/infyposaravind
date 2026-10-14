
var app = angular.module('InfinityLab');

app.controller('BOMInwardsController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', 'cntxService', '$timeout', 'Excel',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, cntxService, $timeout, Excel) {
        $rootScope.AccordionName = $location.path();
        $scope.Navigation = $rootScope.GetNavigation();

        $scope.intercompanytransfer = false;

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });
        $scope.ShowSearch = false;
        $scope.SearchResults = [];
        $scope.SearchBOMOutwardResult = [];
        $scope.SearchRef = restService.SearchReference;

        $scope.CompanyList = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.CompanyList = results;
        });

        $scope.Location = restService.Get('ReferencelistService/FULL-LOCATION', true).then(function (results) {
            $scope.Location = results;
        });

        $scope.BOMCategoryList = restService.Get('ReferencelistService/BOMCATEGORY', true).then(function (results) {
            $scope.BOMCategoryList = results;
        });

        $scope.BOMAllList = restService.Get('ReferencelistService/BOMLIST', true).then(function (results) {
            $scope.BOMAllList = results;
        });
        $scope.ColorList = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColorList = results;
        });
        $scope.LocalContext = cntxService.CreatePageContext('outward');
        $scope.TotalAccepted = 0;
        $scope.TotalCounts = 0;
        $scope.SearchCounts = 0;
        $scope.Entity = {
            "id": 0,
            "items_json": null,
            "selectAll": false,
            "code": null,
            "companyid": 0,
            "fromlocationid": 0,
            "tolocationid": 0,
            "fromdepartmentid": 0,
            "todepartmentid": 0,
            "packedby": 0,
            "receivedby": 0,
            "packedon": null,
            "notes": null,
            "approvedby": 0,
            "isactive": true,
            "createdon": "",
            "modifiedon": "",
            "createdby": 0,
            "modifiedby": 0,
            "status": 0,
            "qty": 0,
            "version": 1,
            "cansave": true,
            "candelete": false,
            "items": {},
            "checkList": true,
            tocompanyid: ""
        };
        $scope.BOMInwards = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.BOMInwards.items = [];

        $scope.SaveOutList = function () {
            if (typeof ($scope.BOMInwards.items) == 'undefined' || $scope.BOMInwards.items == null)
                $scope.BOMInwards.items = [];

            $scope.BOMInwards.items.push(
                {
                    'bomcategoryid': $scope.BOMInwards.items.bomcategoryid,
                    'bomid': $scope.BOMInwards.items.bomid,
                    'qty': $scope.BOMInwards.items.qty
                    //'qty': $scope.BOMInwards.salablegoods.qty,
                    //'department': $scope.BOMInwards.salablegoods.department,
                    //'division': $scope.BOMInwards.salablegoods.division
                });
            $scope.OutConfig();
        }

        $scope.OutConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '200px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.options.autoEdit = true;
            igrid.AddReferenceColumn('bomcategoryid', 'BOM Category', 220, $scope.BOMCategoryList);
            igrid.AddReferenceColumn('bomid', 'BOM Type', 220, $scope.BOMAllList);
            igrid.AddReferenceColumn('colourid', 'Colour', 100, $scope.AllColorList);
            igrid.AddSimpleColumn('bomcode', 'Code', 80);
            igrid.AddSimpleColumn('width', 'Width', 80);
            igrid.AddFormatedColumn('qty', 'Qty', 100, 'N2', 2);
            var column = igrid.AddColumn(
                {
                    id: "aqty", name: "Accepted", field: "aqty",
                    width:100, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );

            var commands = "";
            var commands = "";
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            if ($rootScope.UserInfo.p_PRODUCTMASTER.modify || $rootScope.UserInfo.p_PRODUCTMASTER.delete) {
                igrid.AddButtonColumn('action', 'ACTION', 150, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Delete')
                            $scope.ManageList(args.item);
                    });
                });
            }
            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.BOMInwards.items);
        }

        $scope.AcceptAllQty = function () {
            $.each($scope.BOMInwards.items, function (k, v) {
                v.aqty = v.qty;
            });
            $scope.igrid.UpdateData($scope.BOMInwards.items);
            $scope.Save();
        }

        $scope.ManageList = function (item) {
            $scope.BOMInwards.items.splice($scope.BOMInwards.items.indexOf(item), 1);
            $scope.igrid.UpdateData($scope.BOMInwards.items);
        };

        $scope.PrintList = [];
        $scope.ShowPrint = true;
        $scope.totalqty = 0;

        $scope.PushItems = function () {
            var removeitem = [];
            $.each($scope.SearchResults, function (key, value) {
                if (value.selected) {

                    var existingitem = $scope.BOMInwards.items.find(function (item) {
                        return item.stockid == value.stockid;
                    })

                    if (typeof (existingitem) == 'undefined' || existingitem == null) {
                        value.iqty = value.qty;
                        $scope.BOMInwards.items.push(value);
                    }
                    removeitem.push(value);
                }
            });


            $scope.BOMInwards.packedon = new Date();


            $scope.TotalCounts = 0;
            $.each($scope.BOMInwards.items, function (k, v) {
                $scope.TotalCounts += v.qty;
            });


            $.each(removeitem, function (k, v) {
                var index = $scope.SearchResults.indexOf(v);
                $scope.SearchResults.splice(index, 1);
                v.selected = false;
            });

            $scope.SearchCounts = 0;
            $.each($scope.SearchResults, function (k, v) {
                $scope.SearchCounts += (v.ismeter ? 1 : v.qty);;
            });

            $scope.LocalContext.Post('$scope.BOMInwards.items', $scope.BOMInwards.items, false);
        };

        $scope.fileuploadedcomplete = function (value) {
            var barcodelist = [];
            $.each(value.rows, function (k, v) {
                barcodelist.push(v.values[0]);
            });
            $scope.SearchStock(true, barcodelist.join(','));
        };


        $scope.PopItems = function () {
            var removeitem = [];
            $.each($scope.BOMInwards.items, function (key, value) {
                if (value.selected) {
                    $scope.SearchResults.push(value);
                    removeitem.push(value);
                }
            });

            $.each(removeitem, function (k, v) {
                $scope.BOMInwards.items.splice($scope.BOMInwards.items.indexOf(v), 1);
                v.selected = false;
            });

            $scope.TotalCounts = 0;
            $.each($scope.BOMInwards.items, function (k, v) {
                $scope.TotalCounts += v.qty;;
            });

            $scope.SearchCounts = 0;
            $.each($scope.SearchResults, function (k, v) {
                $scope.SearchCounts += (v.ismeter ? 1 : v.qty);;
            });
        };

        $scope.SearchOutwords = {
            'fromwarehouseBOMOutward': true, 'todate': null, 'fromdate': null
        };

        $scope.SearchCondition = {
            id: 0,
            date: null,
            addedfrom: null,
            addedto: null,
            productid: null,
            brandid: null,
            barcode: null,
            companyid: null,
            searchnewbarcodes: false,
            searchtype: 4,
            supplierid: [],
            suppliername: '',
            lrentryno: null,
            selected: false
        }

        $scope.LastSaved = function () {
            var value = $scope.LocalContext.Get('$scope.BOMInwards.items', false);
            if (typeof (value) != 'undefined' && value == null) {
                $rootScope.ShowAlert('No data found', 'No-Data');
                return;
            }
            $scope.BOMInwards.items = value;
            $scope.TotalCounts = 0;
            $.each($scope.BOMInwards.items, function (k, v) {
                $scope.TotalCounts += v.qty;;
            });

        };

        $scope.currentcompanyid = 0;
        $scope.SearchStock = function (barcoded, barcodelist) {

            if ($scope.Appsetting_Company &&
                (typeof ($scope.BOMInwards.companyid) == 'undefined' || $scope.BOMInwards.companyid <= 0)) {
                $rootScope.Toaster.Info(" Please select company");
                return;
            }

            if ($scope.currentcompanyid == 0) {
                $scope.currentcompanyid = $scope.BOMInwards.companyid;
            }
            else if ($scope.currentcompanyid != $scope.BOMInwards.companyid) {
                $scope.currentcompanyid = $scope.BOMInwards.companyid;
                $scope.SearchResults = [];
                $scope.BOMInwards.items = [];
            }


            var request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            if (barcoded) {
                request = {
                    "Item": { 'barcode': barcodelist, 'companyid': $scope.BOMInwards.companyid, 'searchtype': 5 },
                    "securitytoken": "String content"
                }
            }
            else {
                $scope.SearchCondition.companyid = $scope.BOMInwards.companyid;
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMBundlesService/SearchStock",
                data: srequest
            }).success(function (result) {

                if (angular.isArray(result)) {

                    $scope.SearchResults = [];

                    $.each(result, function (k, v) {

                        if (barcoded) {
                            var itm = $scope.BOMInwards.items.find(function (itm) {
                                return v.stockid == itm.stockid;
                            });
                            if (typeof (itm) == 'undefined' || itm == null) {
                                //$scope.BOMInwards.items.push(v);
                                $scope.BOMInwards.items.splice(0, 0, v);
                                $scope.SearchCondition.directbarcode = "";
                                if (!v.ismeter && $scope.BOMInwards.multiscan) {
                                    v.iqty = 1;
                                }

                                $scope.LocalContext.Post('$scope.BOMInwards.items', $scope.BOMInwards.items, false);
                            }
                            else {
                                if (!v.ismeter && $scope.BOMInwards.multiscan) {
                                    itm.iqty += 1;
                                }
                                else {
                                    itm.qty = v.qty;
                                    itm.iqty = v.iqty;
                                }
                                $scope.SearchCondition.directbarcode = "";
                                $scope.LocalContext.Post('$scope.BOMInwards.items', $scope.BOMInwards.items, false);
                            }
                        }
                        else {
                            var itm = $scope.SearchResults.find(function (itm) {
                                return v.stockid == itm.stockid;
                            });
                            if (typeof (itm) == 'undefined' || itm == null)
                                $scope.SearchResults.push(v);

                            if ($scope.SearchCondition.searchtype == 5) {
                                $scope.SearchCondition.barcode = "";
                            }
                        }

                    });
                }


                $scope.SearchCounts = 0;
                $scope.TotalCounts = 0;
                $.each($scope.SearchResults, function (k, v) {
                    $scope.SearchCounts += (v.ismeter ? 1 : v.qty);
                });
                $scope.CalculateTotal();

            }).error(function (result) {
            });
        }

        $scope.CalculateTotal = function () {
            $scope.TotalCounts = 0;
            $.each($scope.BOMInwards.items, function (k, v) {
                $scope.TotalCounts += v.qty;
            });
        }

        $scope.selectAllItems = function (items, mode) {

            $.each(items, function (key, value) {
                value.selected = mode;
            });

            /*if ($scope.BOMInwards.selectAll) {
                $.each($scope.SearchResults, function (key, value) {
                    $scope.SearchResults[key].selected = true;
                });
            }
                else {
                    $.each($scope.SearchResults, function (key, value) {
                        $scope.SearchResults[key].selected = false;
                    });
            }*/

        }

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.BOMInwards = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ErrorMessage = "";

        $scope.Clear = function () {
            $scope.BOMInwards = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.SearchResults = [];
            $scope.BOMInwards.items = [];
            $scope.TotalAccepted = 0;
            $scope.TotalCounts = 0;
        };
        $scope.SaveRequest = false;
        $scope.Save = function () {
            var iserror = false;
            var wrongbarcode = '';
            $scope.TotalCounts = 0;
            //$.each($scope.BOMInwards.items, function (k, v) {
            //    if (v.qty < v.iqty || v.iqty <= 0) {
            //        wrongbarcode = v.barcode;
            //        iserror = true;
            //    }
            //});

            if (iserror) {
                $rootScope.ShowAlert('No data found', 'Wrong Out qty for - ' + wrongbarcode);
                return;
            }

            if ($scope.BOMInwards.packedon == null) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Info(" Please select date");
                return;
            }
        

            if ($scope.BOMInwards.packedby == null || $scope.BOMInwards.packedby == 0) {
                $scope['setFocuspackedby']();
                $rootScope.Toaster.Info("Please select packedby");
                return;
            }
            if ($scope.BOMInwards.fromlocationid == null || $scope.BOMInwards.fromlocationid == 0) {
                $scope['setFocusfromlocationid']();
                $rootScope.Toaster.Info("Please select fromlocation");
                return;
            }

            if ($scope.BOMInwards.tolocationid == null || $scope.BOMInwards.tolocationid == 0) {
                $scope['setFocustolocationid']();
                $rootScope.Toaster.Info("Please select tolocation");
                return;
            }


            if ($scope.SaveRequest) return;
            $scope.SaveRequest = true;
            request = {
                "Item": $scope.BOMInwards,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMBundlesService/Inward",
                data: srequest
            }).success(function (result) {
                $scope.SaveRequest = false;
                if (result != null && result.id > 0) {
                    $scope.LocalContext.Post('$scope.BOMInwards.items', null, false);
                    $rootScope.Toaster.Info("BOMInwards Saved Successfully.!");
                    $scope.LoadBOMOutward(result);
                }
                else {
                    $rootScope.Toaster.Error(result.errormessage);
                }

            }).error(function (result) {
                $scope.SaveRequest = false;
            });
        }

        $scope.SearchByCode = function () {
            $scope.SaveRequest = false;
            request = {
                "Item": {
                    'code': $scope.BOMInwards.code,
                    'fromwarehouseoutward': true
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMBundlesService/Search",
                data: srequest
            }).success(function (result) {
                if ($.isArray(result) && result.length > 0) {
                    $scope.LoadBOMOutward(result[0]);
                }
                else {
                    $scope.BOMInwards.statusdisplay = 'Not Found!';
                }
            }).error(function (result) {
            });
        }

        $scope.LoadBOMOutward = function (item) {
            $scope.BOMInwards = item;
            $scope.BOMInwards.cansave = false;
            $scope.BOMInwards.candelete = $scope.BOMInwards.status == 0
            $scope.BOMInwards.statusdisplay = '';
            switch ($scope.BOMInwards.status) {
                case 0:
                    $scope.BOMInwards.statusdisplay = 'New Bundle';
                    break;
                case 1:
                    $scope.BOMInwards.statusdisplay = 'Inwarded Bundle';
                    break;
                case 2:
                    $scope.BOMInwards.statusdisplay = 'Inwarded Bundle with changes';
                    break;
                case 9:
                    $scope.BOMInwards.statusdisplay = 'Rejected Bundle';
                    break;
            }

            if ($scope.BOMInwards.status == 9)
                $scope.BOMInwards.id = 0;

            $scope.TotalCounts = 0;
            $scope.TotalAccepted = 0;
            $.each($scope.BOMInwards.items, function (k, v) {
                v.sno = k + 1;
                $scope.TotalCounts += (v.qty);
                if (v.aqty > 0)
                    $scope.TotalAccepted += (v.aqty);
            });
            $scope.Navigation.SetMode('EDIT');
            $scope.ShowSearch = false;
            $scope.OutConfig();
        }

        $scope.AQtyChanged = function () {
            $scope.TotalAccepted = 0;
            $.each($scope.BOMInwards.items, function (k, v) {
                if (v.aqty > 0)
                    $scope.TotalAccepted += v.aqty;
            });
        }
        $scope.ChangeSelectAll = function (selected) {
            $scope.TotalAccepted = 0;
            $.each($scope.BOMInwards.items, function (k, v) {
                v.aqty = selected ? v.qty : 0;
                if (v.aqty > 0)
                    $scope.TotalAccepted += v.aqty;
            });
        }
        
        $scope.ManageSearchItem = function (action, item) {

            if (action == 'Delete') {
                request = {
                    "Item": item,
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "BOMBundlesService/Delete",
                    data: srequest
                }).success(function (result) {

                    if (result) {
                        $rootScope.Toaster.Info("Successfully Removed");
                    }
                    else {
                        $rootScope.Toaster.Info("Some of the items already sold, can't reject");
                    }

                }).error(function (result) {
                });
            } else if (action == "Edit") {
                $scope.LoadBOMOutward(item);
            }
        }


        $scope.Delete = function () {
            request = {
                "Item": $scope.BOMInwards,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BOMBundlesService/Delete",
                data: srequest
            }).success(function (result) {

                if (result) {
                    $scope.statusdisplay = "Successfully Removed";
                    $scope.ErrorStyle = "success";
                    $scope.BOMInwards = (JSON.parse(JSON.stringify($scope.Entity)));
                    $scope.TotalCounts = 0;
                    $scope.SearchCounts = 0;
                    $scope.SearchResults = [];
                }
                else {
                    $scope.statusdisplay = "Some of the items already sold, can't reject";
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
            });

        }

        $scope.CanReject = false;

        $scope.Reject = function () {
            request = {
                "Item": $scope.BOMInwards,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BOMBundlesService/Reject",
                data: srequest
            }).success(function (result) {

                if (result) {
                    $scope.ErrorMessage = "Successfully Removed";
                    $scope.ErrorStyle = "success";
                }
                else {
                    $scope.ErrorMessage = "Some of the items already sold, can't reject";
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
            });
        }

        $scope.Clear();

        var pagesize = 20;

        $scope.PrintPage = function () {
            $timeout(function () {

                var otitle = angular.element('#apptitle').context.title;
                var titlele = angular.element('#apptitle').context.title = "BOMInwards SLIP";

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
        }

        $scope.exporttoExcel = function (tableid, title) {
            $scope.exportHref = Excel.tableToExcel(tableid, title, 1, 'BOMOutward');
            if ($scope.exportHref != null) {
                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = "BOMOutward.xls";
                    link.href = $scope.exportHref;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
        }

        $scope.PrintPDF = function (item) {
            $timeout(function () {
                var link = document.createElement('a');
                link.download = "debitnote.pdf";
                link.href = 'BOMBundlesService/PrintInPDF/' + $scope.UserInfo.OrganizationID + '/' + $scope.BOMInwards.id;
                document.body.appendChild(link);
                link.click();
                $timeout(function () {
                    link.remove();
                }, 100);
            }, 10);
        }

        $scope.PrintDocumentList = [];
        $scope.PrintOW = function () {
            $scope.totalqty = 0.0;

            request = {
                "Item": { 'id': $scope.BOMInwards.id },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMBundlesService/Print",
                data: srequest
            }).success(function (result) {
                $scope.PrintDocument = result;
                $scope.PrintDocumentList = [];
                var items = [];

                $.each(result.items, function (k, v) {
                    var exist = items.find(function (item) {
                        return item.productid == v.productid;
                    });
                    if (exist == null) {
                        var item = (JSON.parse(JSON.stringify(v)));
                        items.push(item);
                    }
                    else {
                        exist.iqty += v.iqty;
                    }
                });

                $.each(items, function (k, v) {
                    $scope.PrintDocumentList.push(v);
                });

                $.each($scope.PrintDocumentList, function (k, v) {
                    $scope.totalqty += v.iqty;
                });
                $scope.PrintPage();

            }).error(function (result) {
            });
        }

        $scope.Appsetting_Company = false;
        //$scope.LoadApplicationSetting = function () {
        //    if (typeof ($rootScope.UserInfo) != 'undefined' &&
        //        typeof ($rootScope.UserInfo.appsettings) != 'undefined') {
        //        $scope.Appsetting_Company = true;
        //    }
        //    else {
        //        $timeout(function () {
        //            $scope.LoadApplicationSetting();
        //        }, 50);
        //    }
        //}


        $timeout(function () {
            //$scope.LoadApplicationSetting();
            $scope.setFocusfirst();
        }, 10);
        $scope.CreateNew = function () {
            $scope.BOMInwards.packedon = new Date();
        }();
    }]);
