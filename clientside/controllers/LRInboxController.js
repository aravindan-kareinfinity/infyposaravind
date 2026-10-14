
var app = angular.module('InfinityLab');

app.controller('LRInboxController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$state', '$timeout','$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $state, $timeout, $filter) {
        $scope.SearchRef = restService.SearchReference;
        $scope.filteredtype = '';
        $scope.Transports = [];
        $scope.City = [];
        $scope.LRInboxList = [];
        $scope.Suppliers = [];
        $scope.Company = [];
        $scope.Location = [];
        $scope.CourierAttachment = false;
        /*LREntry,
            Courier,
            LRIssue,
            LRReceived,
            Package*/
        $scope.ItemTypes = [{ 'id': 0, 'name': 'ALL' }, { 'id': 2, 'name': 'Courier' }, { 'id': 1, 'name': 'LR Entry' }, { 'id': 8, 'name': 'Lorry Delivery' }, { 'id': 7, 'name': 'Hand Delivery' },
        { 'id': 3, 'name': 'LR Issue' }, { 'id': 4, 'name': 'LR Received' }, { 'id': 9, 'name': 'Purchase Return' }, { 'id': 5, 'name': 'Package' }, { 'id': 6, 'name': 'Pending' }];

        /*
            Open=0,
            Working = 1,
            Completed = 2,

            New_LR=200,
            Invoice_Preparation=201,
            Items_Entering=202,
            Barcode_Generating=203,

            //Courier
            Received = 100,
            Dispatched = 101,
            Send = 102,
            Sent = 103,*/

        restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $.each(results, function (k, v) {
                $scope.Company.push(v);
            });

        });

        $scope.Agents = restService.Get('ReferencelistService/AGENT', true).then(function (results) {
            $scope.Agents = results;
        });
      

        restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $.each(results, function (k, v) {
                $scope.Location.push(v);
            })
        });

        restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
            $.each(results, function (k, v) {
                $scope.Transports.push(v);
            })
            //$scope.PreLoad();
        });

        $scope.gettransportname = function (id) {
            try {
                var found = $scope.Transports.find(function (arritem) {
                    return arritem.id == id;
                });
                if (typeof (found) != 'undefined' && found != null)
                    return found.name;
            } catch (e) {
                //alert(e);
            }
        }

        $scope.StatusList = [{ 'id': 0, 'name': 'Open' }, { 'id': 1, 'name': 'Working' }, { 'id': 2, 'name': 'Completed' }, { 'id': 3, 'name': 'Cancelled' },
        { 'id': 100, 'name': 'Received' }, { 'id': 101, 'name': 'Dispatched' }, { 'id': 102, 'name': 'Send' },
        { 'id': 103, 'name': 'Sent' }, { 'id': 104, 'name': 'Received HOLD' },
        { 'id': 200, 'name': 'New LR' },
        { 'id': 201, 'name': 'LR Issue' }, { 'id': 202, 'name': 'LR Received' },
        { 'id': 203, 'name': 'Invoice Preparation' }, { 'id': 204, 'name': 'Items Entering' },
            { 'id': 205, 'name': 'Barcode Generating' },
            { 'id': 250, 'name': 'Ready for Barcode' }];

        $scope.typechanged = function () {
            if ($scope.SearchRequest.typeid == 6 || $scope.SearchRequest.typeid == 7 || $scope.SearchRequest.typeid == 0) {
                $scope.filteredtype = '';
            }
            else {
                var ftype = $scope.ItemTypes.find(function (item) {
                    return $scope.SearchRequest.typeid == item.id;
                });
                if (ftype != null && ftype.id > 0)
                    $scope.filteredtype = ftype.name;
                else
                    $scope.filteredtype = '';
            }
        };


        $scope.supplierchanged = function () {

            var ftype = $scope.Suppliers.find(function (item) {
                return $scope.SearchRequest.supplierid == item.id;
            });
            if (ftype != null && ftype.id > 0)
                $scope.filteredtype = ftype.name;
            else
                $scope.filteredtype = '';

        };

        $scope.statuschanged = function () {
            if ($scope.SearchRequest.statusid < 200) {
                $scope.filteredtype = '';
            }
            else {
                var ftype = $scope.StatusList.find(function (item) {
                    return $scope.SearchRequest.statusid == item.id;
                });
                if (ftype != null && ftype.id >= 200)
                    $scope.filteredtype = ftype.name;
                else
                    $scope.filteredtype = '';
            }
        };

        $scope.SearchRequest = {
            typeid: 0,
            addedfrom: null,
            supplierid: null,
            statusid: null,
            addedto: null
        };

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });
        restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $.each(results, function (k, v) {
                $scope.City.push(v);
            })
        });

        $scope.Search = function () {
            $scope.LRInboxList = [];
            request = {
                "Item": $scope.SearchRequest,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "InboxService/Search",
                data: srequest
            }).success(function (result) {
                $scope.LRInboxList = result;
                if ($scope.igrid)
                    $scope.igrid.UpdateData($scope.LRInboxList);
                else
                    $scope.ConfigGrid();
                $scope.CourierAttachment = false;
            }).error(function (result) {
            });
        }



        $scope.ManageLRInboxList = function (item) {
            if (item.typeid == 2) {
                $location.url('/Courier?id=' + item.referenceid);
            }
            else if (item.typeid == 1) {
                $location.url('/LREntry?id=' + item.referenceid)
            }
            else if (item.typeid == 3) {
                $location.url('/IssueLR?id=' + item.referenceid);
            }
            else if (item.typeid == 4) {
                $location.url('/ReceiptLR?id=' + item.referenceid);
            }
            else if (item.typeid == 7) {
                $location.url('/PurchaseReturn?id=' + item.referenceid);
            }
        };

        $scope.GetSelecteItemObject = function (type) {
            var seletedRows = this.igrid.GetSelectedRows();
            var retList = [];
            $.each(seletedRows, function (k, v) {
                if ((type == null || v.typeid == type) && (typeof (statusid) == 'undefined' || statusid == null || v.statusid == statusid) && (typeof (modeofdelivery) == 'undefined' || v.modeofdelivery == modeofdelivery))
                    retList.push(v);
            });
            return retList;
        };

        $scope.GetSelecteItems = function (type, statusid, modeofdelivery) {
            var seletedRows = this.igrid.GetSelectedRows();
            var retList = [];
            $.each(seletedRows, function (k, v) {
                if ((type == null || v.typeid == type) && (typeof (statusid) == 'undefined' || statusid == null || v.statusid == statusid) && (typeof (modeofdelivery) == 'undefined' || v.modeofdelivery == modeofdelivery))
                    retList.push(v.referenceid);
            });
            return retList;
        };

        $scope.GetSelecteItemsstatus = function () {

            var seletedRows = this.igrid.GetSelectedRows();

            var retList = [];
            $.each(seletedRows, function (k, v) {
                retList.push(v);
            });
            return retList;
        };

        $scope.LRIssue = function () {
            var list = $scope.GetSelecteItems(1, 200, 1);
            if (list.length > 0) {
                $location.url('/IssueLR?ids=' + list.join());
            }
            else {
                $rootScope.ShowAlert('InBox', "Please select Lorry - LR, Which don't have LR Issues.");
            }
        };

        $scope.LRReceipt = function () {
            var list = $scope.GetSelecteItems(3);
            if (list.length == 1) {
                $location.url('/ReceiptLR?ids=' + list[0]);
            }
        };

        $scope.LRInvoice = function () {
            var list = $scope.GetSelecteItemObject(1);
            if (list.length == 1) {
                if (!$scope.system_supportpricingflow && list[0].modeofdelivery == 1 && (list[0].statusid < 202 && !(list[0].statusid == 1 || list[0].statusid == 2))) {
                    $rootScope.ShowAlert('InBox', "Lorry delivery can't allow invoice entry LR has issue & receive.");
                    return;
                }
                $location.url('/Invoice?lrid=' + list[0].referenceid);
            }
        };

        $scope.CheckReceipt = function (item, url) {
            restService.Get('InboxService/LRReceived/' + item.referenceid, true).then(function (results) {
                if (results &&
                    results.length &&
                    results.length > 0) {
                    $location.url(url);
                }
                else {
                    $rootScope.ShowAlert('InBox', "Lorry delivery can't allow enter invoice items until LR has issue & receive.");
                }
            });
        }

        $scope.LRProducts = function () {
            var list = $scope.GetSelecteItemObject(1);
            if (list.length == 1) {

                if (list[0].modeofdelivery == 1 && $scope.system_supportpricingflow) {
                    $scope.CheckReceipt(list[0], '/LRInvoiceItems?lrid=' + list[0].referenceid);
                    return;
                }
                if (list[0].modeofdelivery == 1 && list[0].statusid < 202 && !(list[0].statusid == 1 || list[0].statusid == 2)) {
                     $rootScope.ShowAlert('InBox', "Lorry delivery can't allow enter invoice items until LR has issue & receive.");
                    return;
                }
                else if (list[0].statusid < 203 && !(list[0].statusid == 1 || list[0].statusid == 2)) {
                    $rootScope.ShowAlert('InBox', "Pleae enter invoice before entering invoice items.");
                    return;
                }
                $location.url('/LRInvoiceItems?lrid=' + list[0].referenceid);
            }
        };

        $scope.LRPrice = function () {
            var list = $scope.GetSelecteItemObject(1);
            if (list.length == 1) {
                if (!$scope.system_supportpricingflow && list[0].modeofdelivery == 1 && list[0].statusid < 202 && !(list[0].statusid == 1 || list[0].statusid == 2)) {
                    $rootScope.ShowAlert('InBox', "Lorry delivery can't allow enter invoice items until LR has issue & receive.");
                    return;
                }
                else if (list[0].statusid < 203 && !(list[0].statusid == 1 || list[0].statusid == 2)) {
                    $rootScope.ShowAlert('InBox', "Pleae enter invoice before entering invoice items.");
                    return;
                }
                $location.url('/LRInvoicePrice?lrid=' + list[0].referenceid);
            }
        };

        $scope.LRPOGrn = function () {
            var list = $scope.GetSelecteItemObject(1);
            if (list.length == 1) {
                if (!$scope.system_supportpricingflow && list[0].modeofdelivery == 1 && list[0].statusid < 202 && !(list[0].statusid == 1 || list[0].statusid == 2)) {
                    $rootScope.ShowAlert('InBox', "Lorry delivery can't allow enter invoice items until LR has issue & receive.");
                    return;
                }
                else if (list[0].statusid < 203 && !(list[0].statusid == 1 || list[0].statusid == 2)) {
                    $rootScope.ShowAlert('InBox', "Pleae enter invoice before entering invoice items.");
                    return;
                }
                $location.url('/LRPOGrn?lrid=' + list[0].referenceid);
            }
        };

        $scope.ProductBarcode = function () {
            var list = $scope.GetSelecteItemObject(1);
            if (list.length == 1) {
                if (list[0].modeofdelivery == 1 && $scope.system_supportpricingflow) {
                    $scope.CheckReceipt(list[0], '/barcode?lrid=' + list[0].referenceid);
                    return;
                }
                else if (list[0].modeofdelivery == 1 && list[0].statusid < 202 && !(list[0].statusid == 1 || list[0].statusid == 2)) {
                    $rootScope.ShowAlert('InBox', "Lorry delivery can't generate barcode until LR has issue & receive.");
                    return;
                } else if (list[0].statusid < 204 && !(list[0].statusid == 1 || list[0].statusid == 2)) {
                    $rootScope.ShowAlert('InBox', "Pleae enter invoice items before generating barcodes.");
                    return;
                }
                $location.url('/barcode?lrid=' + list[0].referenceid);
            }
        };




        $scope.PreLoad = function () {
            request = {
                "Item": {
                    typeid: 0,
                    addedfrom: null,
                    addedto: null,
                    showallopen: true
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "InboxService/Search",
                data: srequest
            }).success(function (result) {
                if (typeof ($scope.igrid) == 'undefined' || $scope.igrid == null) {
                    $scope.ConfigGrid();
                }
                $scope.igrid.UpdateData($scope.LRInboxList);
                $scope.CourierAttachment = false;
            }).error(function (result) {
            });
        };

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'WAREHOUSE DASHBOARD - INFORMATION';
            infygrid.ExportXLSX(infygrid.grid, title, subtitle);

            //$scope.exportHref = infygrid.ExportExcel(infygrid.grid, title, subtitle);
            //if ($scope.exportHref != null) {
            //    $timeout(function () {
            //        var link = document.createElement('a');
            //        link.download = filename + ".xls";
            //        link.href = $scope.exportHref;
            //        document.body.appendChild(link);
            //        link.click();
            //        link.remove();
            //    }, 100);
            //}
        }

        $scope.StatusChange = function (action) {
            var found = false;
            var items = $scope.GetSelecteItemsstatus();
            if (items.length == 0)
                return;

            var inboxitems = [];
            $.each(items, function (k, v) {
                var inboxstatus = { 'id': v.id, 'hasclosed': false };
                inboxstatus.statusname = action;

                if (action == "Working") {
                    inboxstatus.statusid = 1;
                }
                else if (action == "Completed") {

                    if (v.typedisplay == 'LREntry' && v.statusname != 'Barcode_Generating') {
                        //$rootScope.Toaster.Info("Can't complete the status before the barcode generation.");
                        //return;
                        found = true;
                    }

                    inboxstatus.statusid = 2;
                    inboxstatus.hasclosed = true;
                }
                else if (action == "Cancel") {
                    inboxstatus.statusid = 3;
                    inboxstatus.hasclosed = true;
                }
                inboxitems.push(inboxstatus);
            });

            if (found) {
                $scope.StatusChangeComformation(inboxitems);
            }
            else
                $scope.UpdateStatus(inboxitems);

            //    request = {
            //        "Items": inboxitems,
            //        "SecurityToken": "String content"
            //    }

            //    var srequest = JSON.stringify(request);

            //    $http({
            //        method: 'POST',
            //        url: "InboxService/UpdateStatus",
            //        data: srequest
            //    }).success(function (result) {
            //        $rootScope.Toaster.Info("Status change completed successfully.");
            //        $.each(items, function (k, v) {
            //            $scope.LRInboxList.splice($scope.LRInboxList.indexOf(v), 1);
            //        });
            //    }).error(function (result) {
            //    });
        };

        $scope.UpdateStatus = function (items) {
            request = {
                "Items": items,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "InboxService/UpdateStatus",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Status change completed successfully.");
                $.each(items, function (k, v) {
                    $scope.LRInboxList.splice($scope.LRInboxList.indexOf(v), 1);
                });
            }).error(function (result) {
            });
        }

        $scope.StatusChangeComformation = function (items) {
            $.confirm({
                title: 'Status Change',
                content: 'Some of the LR Entries are not in Barcode generation status. Do you still want to continue status change?',
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
                            $scope.UpdateStatus(items);
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

        $scope.HideSupplier = false;
        $scope.ismulticompany = false;
        $scope.ismultilocationentry = false;
        $scope.system_supportpricingflow = false;
        $scope.system_vendoragreement = false;
        $scope.ConfigGrid = function () {

            if (typeof ($rootScope.UserInfo.appsettings) == 'undefined' || $rootScope.UserInfo.appsettings == null) {
                $timeout(function () {
                    $scope.ConfigGrid();
                }, 50);
                return;
            }

            $scope.HideSupplier = UserDetails.Info && UserDetails.Info.p_HIDESUPPLIER && UserDetails.Info.p_HIDESUPPLIER.view;
            $scope.ismulticompany = $rootScope.UserInfo.appsettings.system_multicompany;
            $scope.ismultilocationentry = $rootScope.UserInfo.appsettings.system_showroompurchase;
            $scope.system_supportpricingflow = $rootScope.UserInfo.appsettings.system_supportpricingflow;
            $scope.system_vendoragreement = $rootScope.UserInfo.appsettings.system_vendoragreement;

            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 20);
            igrid.options.showHeaderRow = true;
            igrid.AddCustomColumn('modeofdelivery', '', 40, function (item) {
                if (item.modeofdelivery <3 && item.msmeno && item.msmeno != null && item.msmeno != '')
                    return "<img style='height:32px;width:32px;padding:4px 4px 4px 4px' src='clientside/dist/img/delivery-msme-" + item.modeofdelivery + ".png' />";
                return "<img style='height:32px;width:32px;padding:4px 4px 4px 4px' src='clientside/dist/img/delivery-" + item.modeofdelivery + ".png' />";
            });

            if ($scope.ismulticompany)
                igrid.AddReferenceColumn('lrcompanyid', 'Company', 100, $scope.Company);
            if ($scope.ismultilocationentry)
                igrid.AddReferenceColumn('entrylocationid', 'Location', 100, $scope.Location);

            igrid.AddSimpleColumn('typedisplay', 'Type', 80);
            igrid.AddSimpleColumn('statusname', 'Status', 100);
            igrid.AddFormatedColumn('addedon', 'Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('lrentryno', 'Entry No', 100);
            igrid.AddSimpleColumn('lrentrydetail', 'LR No', 100);
            igrid.AddFormatedColumn('lrdate', 'LR Date', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('receiveddate', 'LR Recd. Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('invoiceno', 'Inv.No', 75);
            igrid.AddFormatedColumn('invoicedate', 'Inv.Date', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('age', 'Days', 40);
            if (!$scope.HideSupplier) {
                igrid.AddSimpleColumn('suppliername', 'Supplier', 200);
                igrid.AddReferenceColumn('sourcesupplierid', 'Src.Supplier', 100, $scope.Suppliers);
                igrid.AddReferenceColumn('cityid', 'Supplier City', 100, $scope.City);
                igrid.AddReferenceColumn('agentid', 'Agent', 100, $scope.Agents);
            }
            igrid.AddReferenceColumn('transportid', 'Transport', 140, $scope.Transports);
            igrid.AddSimpleColumn('slipno', 'Slip No', 80);
            igrid.AddFormatedColumn('noofbundels', 'No of Bundle', 80, 'N2', 0).total = true;
            igrid.AddFormatedColumn('noofpieces', 'No of Pieces', 80, 'N2', 0).total = true;
            igrid.AddFormatedColumn('totalamount', 'Amount', 80, 'N2', 2).total = true;
            igrid.AddSimpleColumn('section', 'Section', 100);
            igrid.AddSimpleColumn('createdusername', 'Created By', 100);

            var commands = "";
            if ($rootScope.UserInfo.p_LRENTRY && $rootScope.UserInfo.p_LRENTRY.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Manage'/> &nbsp;"

            //commands += "<input type='button' class='btn btn-danger btn-xs' value='Validate'/>"

            igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Manage')
                        $scope.ManageLRInboxList(args.item);
                    else if ($(e.target).val() == 'Validate') {
                        $state.go('PurchaseInsight', {
                            source: 'lrinbox',
                            data: { 'lrid': args.item.referenceid }
                        });
                    }
                });
            });

            //igrid.options.showHeaderRow = true;

            igrid.onSelectedRangesChanged = function (range, rowmodel, data) {
                var selectedrows = $scope.igrid.GetSelectedRows();
                var courierids = [];
                $.each(selectedrows, function (k, v) {
                    if (v.typeid == 2) {
                        courierids.push(v.referenceid);
                    }
                });
                $timeout(function () {
                    $scope.CourierAttachment = courierids.length>0 && selectedrows.length == courierids.length;
                }, 10);
            };
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.LRInboxList);
        }

        $timeout(function () {
            $scope.SearchRequest = {};
            var tdate = new Date();
            tdate = tdate.setDate(tdate.getDate() - 30);
            $scope.SearchRequest.addedfrom = $filter('date')(tdate, datePicker.shortDisplay);
            $scope.SearchRequest.addedto = $filter('date')(new Date(), datePicker.shortDisplay);
            $scope.Search();
        }, 100)

        $scope.DownloadImagePDF = function () {
            var rows = $scope.igrid.GetSelectedRows();
            var ids = [];
            $.each(rows, function (k, v) {
                ids.push(v.referenceid);
            })
            if (ids.length == 0) return;

            request = {
                "item": { 'ids': ids },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CourierService/ImagePDFGeneration",
                data: srequest
            }).success(function (result) {
                if (result && result != null && result.length == 16) {
                    $rootScope.Download("CourierService/ImagePDFGeneration/" + result, 'images.pdf', 'application/pdf');
                    return;
                }
            });
        }

    }]);
