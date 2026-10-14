
var app = angular.module('InfinityLab');

app.controller('InwardsController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog) {
        $rootScope.AccordionName = $location.path();
        $scope.Permission = UserDetails.Info;
        $scope.SearchRef = restService.SearchReference;

        $scope.Modes = [
            { 'id': 0, 'name': 'Auto' },
            { 'id': 1, 'name': 'Manual' }]

        $scope.Location = null;
        restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.Location = results;
        });

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.PriceTags = [];
        restService.Get('ReferencelistService/PRICETAGS', true).then(function (results) {
            $.each(results, function (k, v) {
                if (v.additionalinfo.billdiscountinclude)
                    $scope.PriceTags.push(v);
            });
        });

        $scope.Entity = {
            "id": 0,
            "items_json": null,
            "Items": [],
            "code": null,
            "fromlocationid": 0,
            "tolocationid": 0,
            "packedby": 0,
            "receivedby": 0,
            "packedon": "",
            "receivedon": new Date(),
            "notes": null,
            "approvedby": 0,
            "isactive": true,
            "createdon": "",
            "modifiedon": "",
            "createdby": 0,
            "modifiedby": 0,
            "status": 0,
            "version": 0,
            "cansave": false,
            "showapprove": false,
            "salablegoods": null
        };

        $scope.Inwards = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.system_hidesupplier = false;
        $scope.system_enableinwardmode = false;
        $scope.system_inwardmode = 0;
        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Inwards = (JSON.parse(JSON.stringify($scope.Entity)));
            if ($scope.system_enableinwardmode) {
                $scope.Inwards.mode = $scope.system_inwardmode;
            }
        }

        $scope.ErrorMessage = "";



        $scope.SearchCondition = {
            code: ""
        }

        $scope.Save = function () {
            if ($scope.Inwards.receivedby == null || $scope.Inwards.receivedby == 0) {
                $rootScope.Toaster.Info("Please select received User.")
                return;
            }

            var loc = $scope.Location.find(function (item) {
                return item.id == $scope.Inwards.tolocationid;
            });

            if (typeof (loc) != 'undefined' && loc != null && loc.additionalinfo != null && loc.additionalinfo.iswarehouse) {
                $rootScope.Toaster.Info("You can't inward package to warehouse, use warehouse/receive goods")
                return;
            }

            if ($scope.Inwards.showpackingslip) {
                var nonscannedpackage = $scope.Inwards.packslip.find(function (item) {
                    return item.scanned == undefined || item.scanned == false;
                });
                if (nonscannedpackage && nonscannedpackage != null) {
                    $rootScope.Toaster.Error("Please scan all the packages before inward");
                    return;
                }
            }

            $scope.Inwards.scanmode = $scope.Inwards.mode;
            request = {
                "Item": $scope.Inwards,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: $scope.Inwards.inward2warehouse ? "BundlesService/Inward" : "BundlesService/InwardGoods",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $scope.ErrorMessage = (result.errormessage == 'undefined'
                        || result.errormessage == null
                        || result.errormessage == '' ? "Successfully saved" : result.errormessage);
                    $scope.ErrorStyle = (result.errormessage == 'undefined'
                        || result.errormessage == null
                        || result.errormessage == '' ? "success" : "danger");

                    $scope.Inwards = result;
                    $scope.Inwards.statusdisplay = '';
                    switch ($scope.Inwards.status) {
                        case -1:
                            $scope.Inwards.statusdisplay = 'Partial Bundle Inward Restricted., Please follow Approval process.';
                            break;
                        case 0:
                            $scope.Inwards.statusdisplay = 'New Bundle';
                            break;
                        case 1:
                            $scope.Inwards.statusdisplay = 'Inwarded Bundle';
                            break;
                        case 2:
                            $scope.Inwards.statusdisplay = 'Inwarded Bundle with changes';
                            break;
                        case 8:
                            $scope.Inwards.statusdisplay = 'Partial Bundle Inward Approved.';
                            break;
                        case 9:
                            $scope.Inwards.statusdisplay = 'Rejected Bundle';
                            break;
                    }

                    $scope.Inwards.totaliqty = 0;
                    $scope.Inwards.totalaqty = 0;
                    $scope.Inwards.cansave = $scope.Inwards.status == 0;
                    $.each($scope.Inwards.items, function (k, v) {
                        if ($scope.Inwards.cansave) {
                            v.aqty = v.iqty;
                            v.rowstyle = {};
                        }
                        else if (v.iqty != v.aqty) {
                            v.rowstyle = { "background-color": "lightgrey" };
                        }
                        $scope.Inwards.totaliqty += v.iqty;
                        $scope.Inwards.totalaqty += v.aqty;
                    });
                    $scope.ConfigGrid($scope.Inwards.items);
                }
                else {
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
            });
        }

        $scope.aqtychanged = function (item) {
            if (item.aqty != item.iqty) {
                $scope.Inwards.showapprove = true;
            }
            else {
                $scope.Inwards.showapprove = false;
                $.each($scope.Inwards.Salablegoods, function (k, v) {
                    if (!$scope.Inwards.showapprove && item.aqty != item.iqty)
                        $scope.Inwards.showapprove = true;
                });
            }
            $scope.Retotal();
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
                    confirm: function () {

                        request = {
                            "Item": item,
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "BundlesService/Delete",
                            data: srequest
                        }).success(function (result) {

                            if (result) {
                                $scope.ErrorMessage = "Successfully deleted";
                                $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                            }

                        }).error(function (result) {

                        });
                    }
                });


            }
            else if (action == "Edit") {
                $scope.ShowSearch = false;
                $scope.Inwards = item;
                $scope.Inwards.contactno = parseInt($scope.Inwards.contactno);
                $scope.Inwards.pincode = parseInt($scope.Inwards.pincode);
            }
        }

        $scope.Reject = function () {

            if ($scope.Inwards.receivedby == null || $scope.Inwards.receivedby == 0) {
                $rootScope.Toaster.Info("Please select received/rejected User.")
                return;
            }

            $.confirm({
                title: 'Reject',
                content: 'Do you want to reject the bundle? ',
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
                                "Item": $scope.Inwards,
                                "securitytoken": "String content"
                            }

                            var srequest = JSON.stringify(request);

                            $http({
                                method: 'POST',
                                url: "BundlesService/Reject",
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

        $scope.Retotal = function () {

            $scope.Inwards.totaliqty = 0;
            $scope.Inwards.totalaqty = 0;

            $.each($scope.Inwards.items, function (k, v) {
                $scope.Inwards.totaliqty += v.iqty;
                $scope.Inwards.totalaqty += v.aqty;
            });
        }


        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'Outward Details';
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

        $scope.ShowStock = function () {
            var request = {
                "Item": $scope.Inwards,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BundlesService/UpdateUANStock",
                data: srequest
            }).success(function (result) {

                $.each($scope.Inwards.items, function (k, v) {
                    var itm = result.items.find(function (item) {
                        return item.serialno == v.serialno;
                    });
                    if (itm && itm != null)
                        v.stockqty = itm.rqty;
                });

                $scope.ConfigGrid($scope.Inwards.items);
            });
        }

        $scope.ScanPackingSlip = function () {
            var CurrentPackingSlip = $scope.Inwards.packslip.find(function (item) {
                return item.code == $scope.Inwards.currentpackingcode;
            })
            $.each($scope.Inwards.items, function (k, v) {
                v.sqty = 0;
                v.pqty = 0;
            })
            if (CurrentPackingSlip && CurrentPackingSlip != null) {
                var newitems = [];
                $.each(CurrentPackingSlip.items, function (k, v) {
                    var scanneditem = $scope.Inwards.items.find(function (item) {
                        return item.barcode == v.barcode || item.serialno == v.barcode;
                    });
                    if (scanneditem && scanneditem != null) {
                        scanneditem.pqty = v.qty;
                        scanneditem.sqty = v.aqty == undefined ? 0 : v.aqty;
                        newitems.push(scanneditem);
                    }
                })
                $scope.ConfigGrid(newitems, true);
            }
            else {
                $.each($scope.Inwards.packslip, function (kk, vv) {
                    $.each(vv.items, function (k, v) {
                        var scanneditem = $scope.Inwards.items.find(function (item) {
                            return item.barcode == v.barcode || item.serialno == v.barcode;
                        });
                        if (scanneditem && scanneditem != null) {
                            scanneditem.pqty = v.qty;
                            scanneditem.sqty = v.aqty == undefined ? 0 : v.aqty;
                        }
                    })
                });
                $scope.ConfigGrid($scope.Inwards.items, true);
            }
        }

        $scope.PackingMode = false;
        $scope.GridItems = [];
        $scope.ConfigGrid = function (result, showscan) {
            $scope.GridItems = result;
            $scope.PackingMode = showscan != undefined && showscan;
            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }
            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 30);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial(result);

            var showstock = false;
            if ($scope.IsUANBased) {
                showstock = result.find(function (item) {
                    return item.stockqty && item.stockqty > 0;
                });
                showstock = (showstock && showstock != null);
                igrid.AddSimpleColumn('serialno', 'GLN/UAN', 150);
                igrid.AddSimpleColumn('printingname', 'Item Details', 250);
                igrid.AddSimpleColumn('designid', 'Design', 100);
                igrid.AddSimpleColumn('sizename', 'Size', 80);
            }
            else {
                igrid.AddSimpleColumn('barcode', 'Barcode', 100);

                if ($scope.ShowGLN) {
                    igrid.AddSimpleColumn('serialno', 'GLN/UAN', 100);
                }
                igrid.AddSimpleColumn('printingname', 'Item Details', showscan ? 100 : 140);
            }


            if (!showscan) {
                if (!$scope.IsUANBased) {
                    if (!$scope.system_hidesupplier)
                        igrid.AddSimpleColumn('suppliername', 'Supplier Name', 120);
                    igrid.AddSimpleColumn('lrreference', 'LR Reference', 80);

                    if (typeof ($scope.Permission.p_SHOWVALUE) != 'undefined' && !$scope.Permission.p_SHOWVALUE.view)
                        igrid.AddFormatedColumn('buyingprice', 'Cost Price', 80, 'N2', 2);
                }

                if (typeof ($scope.Permission.p_SHOWSELLINGVALUE) != 'undefined' && !$scope.Permission.p_SHOWSELLINGVALUE.view)
                    igrid.AddFormatedColumn('price', 'Sale Price', 80, 'N2', 2);
            }

            igrid.AddFormatedColumn('mrp', 'MRP', 80, 'N2', 2);

            if ($scope.SupportPriceTypes) {
                igrid.AddSimpleColumn('price', 'Current Price', 100);
                igrid.AddSimpleColumn('uprice', 'Inward Price', 100);
            }

            igrid.AddFormatedColumn('iqty', 'Transfered', 80, 'N2', 2).total = true;

            if (showscan) {
                igrid.AddFormatedColumn('aqty', 'Inward', 60, 'N0', 0).total = true;
                igrid.AddFormatedColumn('pqty', 'Packed', 60, 'N0', 0).total = true;
                igrid.AddFormatedColumn('sqty', 'Scan', 60, 'N0', 0).total = true;
            }
            else {
                if ($scope.Inwards.cansave && !$scope.Inwards.mode)
                    igrid.AddColumn(
                        {
                            id: "In-Ward", name: "Inward", field: "aqty",
                            width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                            'sortable': true
                        }
                    ).total = true;
                else {
                    igrid.AddFormatedColumn('aqty', 'Inward', 60, 'N2', 2).total = true;
                }
                if (showstock) {
                    igrid.AddFormatedColumn('stockqty', 'Stock', 60, 'N2', 2);
                }
            }
            
            igrid.EnableTotal();
            igrid.Initialize();
            igrid.setCellCSS('igridbigrows');

            if (showstock) {
                igrid.dataView.getItemMetadata = function (row) {
                    var item = igrid.dataView.getItem(row);
                    if (item.stockqty == undefined || item.stockqty == null || item.stockqty < item.aqty) {
                        item.style = 'igridbigrowswithred';
                        return { cssClasses: 'igridbigrowswithred' };

                    }
                    else {
                        item.style = '';
                        return { cssClasses: '' };
                    }
                };
            }
            else {

                igrid.dataView.getItemMetadata = function (row) {
                    var item = igrid.dataView.getItem(row);

                    if ($scope.PackingMode) {
                        if (item.aqty + (item.sqty && !isNaN(item.sqty) ? item.sqty : 0) == item.iqty) {
                            item.style = 'igridbigrowswithgreen';
                            return { cssClasses: 'igridbigrowswithgreen' };
                        }
                        else {
                            item.style = 'igridbigrowswithred';
                            return { cssClasses: 'igridbigrowswithred' };
                        }
                    }
                    else {
                        if ($scope.SupportPriceTypes && item.uexist) {
                            item.style = 'igridbigrowswithred';
                            return { cssClasses: 'igridbigrowswithred' };
                        }
                        else {
                            if ($scope.Inwards.cansave && item.iqty != item.aqty) {
                                item.style = 'igriddebitnoterow';
                                return { cssClasses: 'igriddebitnoterow' };
                            } else if ($scope.Inwards.cansave &&
                                item.aqty > item.iqty) {
                                item.style = 'igriddebitnoterow';
                                return { cssClasses: 'igriddebitnoterow' };
                            }
                            else {
                                item.style = 'igridbigrowswithgreen';
                                return { cssClasses: 'igridbigrowswithgreen' };
                            }
                        }
                    }
                };
            }
            igrid.UpdateData(result, '_id');
            $scope.igrid = igrid;
        }

        $scope.ScanModeChanged = function () {
            if (!$scope.Inwards.cansave) return;

            $.each($scope.Inwards.items, function (k, v) {
                if ($scope.Inwards.mode == 0) {
                    v.aqty = v.iqty;
                    v.rowstyle = {};
                }
                else {
                    v.rowstyle = {};
                    v.aqty = 0;
                }
            });
            $scope.ConfigGrid($scope.Inwards.items);
        }

        $scope.ClearScan = function () {
            $.each($scope.GridItems, function (k, v) {
                v.sqty = 0;
            });
            $scope.ConfigGrid($scope.GridItems,true);
        }

        $scope.UpdateMeterQty = function (item) {
            $rootScope.Prompt('Inward', 'Meter', function (count) {
                var totalcount = $rootScope.GetNumber(count);
                item.aqty = totalcount;

                $timeout(function () {
                    $scope.SearchCondition.directbarcode = '';
                    $scope.igrid.UpdateData($scope.GridItems);
                    $scope.setFocusdirectbarcode();
                }, 50);

            });
        }

        $scope.SearchStock = function (scan, barcode) {
            if (barcode == 'undefined' || barcode == null || barcode == '') return;

            var found = $scope.Inwards.items.find(function (item) {
                return ((item.barcode == barcode || item.serialno == barcode)
                    && (item.aqty == 'undefined' || item.aqty == null || item.aqty == 0 || item.iqty > item.aqty));
            });

            if (typeof (found) != 'undefined' && found != null) {
                if ($scope.PackingMode) {
                    if (found.sqty)
                        found.sqty += 1;
                    else
                        found.sqty = 1;

                    if (found.sqty > found.pqty) {
                        $rootScope.Toaster.Info("Scanned qty exceed packed qty");
                    }
                }
                else {

                    if (found.ismeter) {
                        $scope.UpdateMeterQty(found);
                    }
                    else {
                        if (found.aqty)
                            found.aqty += 1;
                        else
                            found.aqty = 1;
                    }
                }
            }
            else {
                $rootScope.Toaster.Info("Not available or Exceed transfer qty");
            }
            $scope.SearchCondition.directbarcode = '';
            $scope.igrid.UpdateData($scope.GridItems);
            $scope.setFocusdirectbarcode();
        }

        $scope.SavePackingSlip = function () {
            var CurrentPackingSlip = $scope.Inwards.packslip.find(function (item) {
                return item.code == $scope.Inwards.currentpackingcode;
            })
            if (CurrentPackingSlip && CurrentPackingSlip != null) {
                CurrentPackingSlip.scanned = true;
                $.each(CurrentPackingSlip.items, function (k, v) {
                    var scanneditem = $scope.Inwards.items.find(function (item) {
                        return item.barcode == v.barcode || item.serialno == v.barcode;
                    });
                    if (scanneditem && scanneditem != null) {
                        v.aqty += scanneditem.sqty;
                    }
                })
                $scope.Inwards.packingcode = CurrentPackingSlip.code;
                request = {
                    "item": $scope.Inwards,
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "BundlesService/UpdatePackingSlip",
                    data: srequest
                }).success(function (result) {
                    $scope.Inwards.currentpackingcode = '';
                });
                $.each($scope.GridItems, function (k, v) {
                    v.aqty += v.sqty;
                    v.sqty = 0;
                })
            }
            $scope.ConfigGrid($scope.Inwards.items);
        }

        $scope.SearchPackage = function () {
            $scope.Inwards.statusdisplay = '';
            $scope.ErrorMessage = '';
            $scope.SearchCondition.validatetolocation = true;
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BundlesService/Search",
                data: srequest
            }).success(function (result) {
                if ($.isArray(result) && result.length > 0) {
                    $scope.Inwards = result[0];
                    $scope.SearchCondition.code = $scope.Inwards.code;
                    $scope.Inwards.showpackingslip = ($scope.Inwards.packslip && $scope.Inwards.packslip != null);
                    $scope.Inwards.totaliqty = 0;
                    $scope.Inwards.totalaqty = 0;
                    $scope.Inwards.cansave = $scope.Inwards.status == 0 || $scope.Inwards.status == 8; // status = 8 For Partial approved.
                    $scope.Inwards.showapprove = false;
                    $scope.Inwards.receivedon = new Date();

                    if ($scope.system_enableinwardmode) {
                        $scope.Inwards.mode = $scope.system_inwardmode;
                    }
                    else
                        $scope.Inwards.mode = 0;

                    var fromlocation = $scope.Location.find(function (item) { return item.id == $scope.Inwards.fromlocationid; });
                    var tolocation = $scope.Location.find(function (item) { return item.id == $scope.Inwards.tolocationid; });
                    //if (fromlocation != null && tolocation != null && fromlocation.additionalinfo != null && tolocation.additionalinfo != null &&
                    //    fromlocation.additionalinfo.parentid > 0 && tolocation.additionalinfo.parentid > 0 &&
                    //    fromlocation.additionalinfo.parentid != tolocation.additionalinfo.parentid) {
                    //    $scope.Inwards.iscompanytransfer = true;
                    //    $scope.Inwards.tocompanyid = tolocation.additionalinfo.parentid;
                    //    $scope.Inwards.fromcompanyid = fromlocation.additionalinfo.parentid;

                    //    $scope.fromcompany = $scope.Company.find(function (item) {
                    //        return item.id == $scope.Inwards.fromcompanyid;
                    //    }).name;

                    //    $scope.tocompany = $scope.Company.find(function (item) {
                    //        return item.id == $scope.Inwards.tocompanyid;
                    //    }).name;
                    //}

                    if ($scope.Inwards.fromcompanyid > 0 && $scope.Inwards.tocompanyid > 0) {
                        $scope.fromcompany = $scope.Company.find(function (item) {
                            return item.id == $scope.Inwards.fromcompanyid;
                        }).name;

                        $scope.tocompany = $scope.Company.find(function (item) {
                            return item.id == $scope.Inwards.tocompanyid;
                        }).name;
                    }

                    $scope.Inwards.canreject = $scope.Inwards.status == 0 || $scope.Inwards.status == 1
                        || $scope.Inwards.status == 2 || $scope.Inwards.status == 8;
                    $.each($scope.Inwards.items, function (k, v) {

                        if ($scope.Inwards.cansave) {
                            v.aqty = v.iqty;
                            v.rowstyle = {};
                        }
                        else if (v.iqty != v.aqty) {
                            v.rowstyle = { "background-color": "lightgrey" };
                        }
                        $scope.Inwards.totaliqty += v.iqty;
                        $scope.Inwards.totalaqty += v.aqty;
                        v.selected = false;
                    });

                    $scope.Inwards.statusdisplay = '';
                    switch ($scope.Inwards.status) {
                        case -1:
                            $scope.Inwards.statusdisplay = 'Un-Approved';
                            break;
                        case 0:
                            $scope.Inwards.statusdisplay = 'New Bundle';
                            break;
                        case 1:
                            $scope.Inwards.statusdisplay = 'Inwarded Bundle';
                            break;
                        case 2:
                            $scope.Inwards.statusdisplay = 'Inwarded Bundle with changes';
                            break;
                        case 8:
                            $scope.Inwards.statusdisplay = 'Partial Bundle Inward Approved.';
                            break;
                        case 9:
                            $scope.Inwards.statusdisplay = 'Rejected Bundle';
                            break;
                    }
                    if ($scope.Inwards.status == 0)
                        $scope.ScanModeChanged();
                    if ($scope.Inwards.showpackingslip)
                        $scope.ScanPackingSlip();
                    else
                        $scope.ConfigGrid($scope.Inwards.items);
                }
            }).error(function (result) {
            });

        }

        $scope.UpdatePrice = function (pricetype) {
            $scope.SearchCondition.validatetolocation = true;
            request = {
                "Item": {
                    'id': $scope.Inwards.id,
                    'tolocationid': $scope.Inwards.tolocationid,
                    'pricetype': pricetype
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BundlesService/FindPrice",
                data: srequest
            }).success(function (results) {
                $.each(results, function (k, v) {
                    var barcodeitem = $scope.Inwards.items.find(function (item) {
                        return item.stockid == v.id;
                    });

                    if (barcodeitem && barcodeitem != null) {
                        barcodeitem.uprice = v.price;
                        barcodeitem.uexist = v.exist;
                    }
                });
                $scope.Inwards.pricetype = pricetype;
                $scope.ConfigGrid($scope.Inwards.items);
            });
        }

        $scope.ChoosePriceType = function (callback) {
            var dialog = ngDialog.open({
                template: 'ChoosePriceType',
                data: {
                    'scope': $scope
                },
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.PriceTags = $scope.ngDialogData.scope.PriceTags;
                    $scope.Selected = function () {
                        $scope.closeThisDialog({
                            'pricetype': $scope.pricetype
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        $scope.UpdatePrice(data.value.pricetype);
                    }
                }
            });
        }

        $scope.PrintBarcode = function () {
            var ids = [];
            var _selectindex = 1;
            $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                if (v.iqty && v.iqty > 0) {
                    ids.push({
                        'barcode': v.barcode,
                        'noofcopy': v.aqty,
                        'selectindex': _selectindex++
                    });
                }
            });

            if (ids.length == 0) {
                $rootScope.Toaster.Info("Please select barcode to be print.");
            }
            $scope.ChoseReport({
                'iswarehouse': false,
                'locationid': $scope.Inwards.tolocationid,
                'barcodes': ids, 'reportid': 0
            });
        };

        $scope.ChoseReport = function (requestitem) {
            $rootScope.Handlers.SelectPrinter('BARCODE', function (printdata) {
                if (printdata == null || printdata.report == null || printdata.report.id <= 0)
                    return;

                requestitem.reportid = printdata.report.id;

                var request = {
                    "Item": requestitem,
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "PricechangeService/PrintBarcode",
                    data: srequest
                }).success(function (result) {
                    if (result != null && result.length > 0) {
                        $rootScope.Sent2Printer(result.join(';'), printdata);
                    }
                });
            });
        }

        $scope.ShowGLN = false;
        $scope.LoadingAppSetting = function () {
            if (typeof ($rootScope.UserInfo.appsettings) == 'undefined' || $rootScope.UserInfo.appsettings == null ||
                $scope.Location == undefined || $scope.Location == null) {
                $timeout($scope.LoadingAppSetting, 100);
            } else {
                $scope.SupportPriceTypes = $rootScope.UserInfo.appsettings.system_invcol_pricetype || false;
                $scope.system_hidesupplier = UserDetails.Info.p_HIDESUPPLIER.view;
                $scope.system_enableinwardmode = $rootScope.UserInfo.appsettings.system_enableinwardmode || false;
                $scope.system_inwardmode = $rootScope.UserInfo.appsettings.system_inwardmode;
                $scope.IsUANBased = $rootScope.UserInfo.appsettings.mfg_mode == 5;
                $scope.ShowGLN = $rootScope.UserInfo.appsettings.system_glnbatch ||
                    $rootScope.UserInfo.appsettings.system_fixed_barcodes;
                if ($scope.system_enableinwardmode) {
                    $scope.Inwards.mode = $scope.system_inwardmode;
                    $scope.ScanModeChanged();
                }

                if (typeof ($location.search().id) != 'undefined' &&
                    $location.search().id.length > 0) {
                    request = {
                        "Item": { 'id': $location.search().id },
                        "SecurityToken": "String content"
                    }
                    $scope.SearchCondition = {};
                    $scope.SearchCondition.validatetolocation = true;
                    $scope.SearchCondition.id = $location.search().id;
                    $scope.SearchPackage();
                }
            }
        }

        $timeout(function () {
            $scope.LoadingAppSetting();
            $scope.setFocusfirst();
        }, 10);




    }]);