
var app = angular.module('InfinityLab');

app.controller('IssueLRController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', '$filter', 'restService', '$timeout', 'Excel',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, $filter, restService, $timeout, Excel) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;
        $scope.BookingOffice = restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
            $scope.BookingOffice = results;
        });

        $scope.Today = new Date();
        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });
        $scope.Company = restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
            $scope.Company = results;
        });
        $scope.CollectedBy = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.CollectedBy = results;
        });

        $scope.City = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.City = results;
            $scope.igrid.UpdateData($scope.IssueLR.lrissuelist, '_id');
        });


        // model to be used to IssueLR
        $scope.Entity = {

            "id": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "",
            "modifiedon": "",
            "bookingofficeid": 0,
            "collectingby": 0,
            "issueno": null,
            "issuedate": "",
            "receivinglocationid": 0,
            "fromlocationid": 0,
            "noofpieces": 0.0,
            "noofbundels": 0.0,
            "frieghtcharges": 0.0,
            "noofboxes": 0.0,
            "weight": 0.0,
            "lrno": 0,
            "entryno": 0,
            "lrissuelist": [],
            "lrissureport": { "Items": [{ "bookingofficeid": null, "lrid": null, "lrno": null, "supplierid": null, "noofbundels": null, "noofpieces": null, "lrissueid": null, "lrentryno": null, "lrdate": null }] }

        };
        $scope.IssueLR = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ReverseLR = function (item) {


            $scope.IssueLR.noofbundels -= item.noofbundels;
            $scope.IssueLR.frieghtcharges -= item.totalamount;
            $scope.IssueLR.noofpieces -= item.noofpieces;
            $scope.IssueLR.noofboxes -= item.noofboxes;
            $scope.IssueLR.weight -= item.weight;
            //var index = $scope.IssueLR.lrissuelist.indexOf(item);
            //$scope.IssueLR.lrissuelist.splice(index, 1);
        }

        $scope.LrSearchCondition = { lrno: "", lrentryno: "" }

        $scope.AddLrItems = function () {
            request = {
                "Item": $scope.LrSearchCondition,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LrentryService/Search",
                data: srequest
            }).success(function (result) {
                // alert(result.items.length);
                if (result.length == 1) {
                    result[0].lrid = result[0].id;
                    result[0].id = 0;

                    $scope.IssueLR.lrissuelist.push(result[0]);
                    $scope.IssueLR.bookingofficeid = result[0].transportid;
                    $scope.IssueLR.noofbundels += result[0].noofbundels;
                    $scope.IssueLR.frieghtcharges += result[0].totalamount;
                    $scope.IssueLR.noofpieces += result[0].noofpieces;
                    $scope.IssueLR.noofboxes += result[0].noofboxes;
                    $scope.IssueLR.weight += result[0].weight;
                    $scope.igrid.UpdateData($scope.IssueLR.lrissuelist,'_id');
                }
                else {

                }
            }).error(function (result) {
            });

        };



        $scope.InvoiceInfo = { invoiceid: $location.search().invoiceid };

        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }

        $scope.ErrorMessage = "";

        $scope.SearchCondition = {
            bookingofficeid: "",
            issuenumber: "",
            issuedate: "",
            fromlocationid: ""
        }

        $scope.Save = function () {
            //if ($scope.IssueLR.bookingofficeid == null || $scope.IssueLR.bookingofficeid == 0) {
            //    $scope['setFocusbookingofficeid']();
            //    $rootScope.Toaster.Info(" Please Select Booking Office");
            //    return;
            //}


            if ($scope.IssueLR.collectingby == null || $scope.IssueLR.collectingby == 0) {
                $scope['setFocuscollectingby']();
                $rootScope.Toaster.Info("Chose the Collect By ");
                return;
            }
            if ($scope.IssueLR.issuedate == null || $scope.IssueLR.issuedate == 0) {
                $scope['setFocusissuedate']();
                $rootScope.Toaster.Info("Please Select Issue Date");
                return;
            }
            //if ($scope.IssueLR.fromlocationid == null ||$scope.IssueLR.fromlocationid == 0) {
            //    $scope['setFocusfromlocationid']();
            //    $rootScope.Toaster.Info("Please select fromlocation");
            //    return;
            //}
            //if ($scope.IssueLR.receivinglocationid == null || $scope.IssueLR.receivinglocationid == 0) {
            //    $scope['setFocusreceivinglocationid']();
            //    $rootScope.Toaster.Info("Please select receivinglocation");
            //    return;
            //}

            request = {
                "Item": $scope.IssueLR,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LrissueService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Lrissue Successfully Saved.! With Issue No :" + result.issueno);
                    $scope.IssueLR.issueno = result.issueno;
                    $scope.IssueLR.id = result.id;
                    $scope.IssueLR.version = result.version;
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
            });

        }

        $scope.printDiv = function () {

            request = {
                "Item": { 'id': $scope.IssueLR.id },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LrissueService/Print",
                data: srequest
            }).success(function (result) {

                $scope.IssueLR.noofbundels = 0;
                $scope.IssueLR.frieghtcharges = 0;
                $scope.IssueLR.noofpieces = 0;

                $scope.PrintDocument = result;

                $.each(result.lrissuelist, function (k, v) {
                    $scope.IssueLR.noofbundels += v.noofbundels;
                    $scope.IssueLR.frieghtcharges += v.totalamount;
                    $scope.IssueLR.noofpieces += v.noofpieces;
                });

                $timeout(function () {

                    var otitle = angular.element('#apptitle').context.title;
                    var titlele = angular.element('#apptitle').context.title = "DEBIT NOT";

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


        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {
                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete issue ' + item.issueno + "?",
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
                                    url: "LrissueService/Delete",
                                    data: srequest
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
                $scope.IssueLR = item;
                $scope.GetLrIssue();
            }
        }
        $scope.GetLrIssue = function () {

            request = {
                "Item": { id: $scope.IssueLR.id },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LrissueService/Search",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length > 0) {
                    $scope.IssueLR = result[0];

                    $scope.IssueLR.noofbundels = 0;
                    $scope.IssueLR.frieghtcharges = 0;
                    $scope.IssueLR.noofpieces = 0;
                    $scope.IssueLR.noofboxes = 0;
                    $scope.IssueLR.weight = 0;

                    $.each(result[0].lrissuelist, function (k, v) {
                        $scope.IssueLR.noofbundels += v.noofbundels;
                        $scope.IssueLR.frieghtcharges += v.totalamount;
                        $scope.IssueLR.noofpieces += v.noofpieces;
                        $scope.IssueLR.noofboxes += v.noofboxes;
                        $scope.IssueLR.weight += v.weight;
                    });
                    $scope.ConfigureGrid();
                    $scope.igrid.UpdateData($scope.IssueLR.lrissuelist, '_id');
                }
            }).error(function (result) {
            });

        }


        $scope.exporttoExcel = function (tableid, title) {
            $scope.exportHref = Excel.tableToExcel(tableid, title, 1, 'LR ISSUE REPORT');
            if ($scope.exportHref != null) {
                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = "lrentrylist.xls";
                    link.href = $scope.exportHref;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
        }

        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LrissueService/Search",
                data: srequest
            }).success(function (result) {
                // alert(result.items.length);
                $scope.SearchResult = result;
                $scope.IssueConfig();
            }).error(function (result) {
            });

        }


        $scope.sgrid = null;
        $scope.IssueConfig = function () {
            if ($scope.sgrid != null)
                $scope.sgrid.destroy();

            var igrid = new InfyGrid('searchgridContainer', '450px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.AddReferenceColumn('bookingofficeid', 'Booking Office', 200, $scope.BookingOffice);
            igrid.AddReferenceColumn('collectingby', 'Collected By', 140, $scope.CollectedBy);

            igrid.AddSimpleColumn('issueno', 'Issue Number', 100);
            igrid.AddFormatedColumn('issuedate', 'Issue Date', 100, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('receivinglocationid', 'Receiving Location', 160, $scope.City);
            //igrid.AddReferenceColumn('supplierid', 'Supplier', 120, $scope.Suppliers);

            var commands = "";
            if ($rootScope.UserInfo.p_LRISSUE.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_LRISSUE.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            if ($rootScope.UserInfo.p_LRISSUE.modify || $rootScope.UserInfo.p_LRISSUE.delete) {
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
            $scope.sgrid = igrid;
            $scope.sgrid.UpdateData($scope.SearchResult, '_id');

        }
        $scope.igrid = null;
        $scope.ConfigureGrid = function () {

            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.AddReferenceColumn('supplierid', 'Supplier Name', 180, $scope.Suppliers);
            igrid.AddReferenceColumn('fromlocationid', 'From Location', 100, $scope.City);

            igrid.AddSimpleColumn('lrentryno', 'Entry No', 80);
            igrid.AddSimpleColumn('lrno', 'LR No', 60);
            igrid.AddFormatedColumn('lrdate', 'LR Date', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('noofbundels', 'No of Bundle', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('noofboxes', 'No of Boxes', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('noofpieces', 'No of Pieces', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('weight', 'Weight', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('totalamount', 'Amount', 80, 'N2', 2).total = true;

            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Remove'/> &nbsp;"


            igrid.AddButtonColumn('action', 'ACTION', 80, commands, function (e, args) {
                $scope.$apply(function () {
                    $scope.ReverseLR(args.item);
                    $scope.IssueLR.lrissuelist.splice($scope.IssueLR.lrissuelist.indexOf(args.item), 1);
                    $scope.igrid.UpdateData($scope.IssueLR.lrissuelist, '_id');
                });
            });
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
        }

        $scope.CreateNew = function () {
            $scope.IssueLR.issuedate = new Date();
        }();

        if (typeof ($location.search().ids) != 'undefined' &&
            $location.search().ids.length > 0) {
            request = {
                "Item": { 'lrids': $location.search().ids },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LrentryService/Search",
                data: srequest
            }).success(function (result) {
                // alert(result.items.length);
                $.each(result, function (k, v) {
                    v.lrid = v.id,
                        v.id = 0
                });

                $scope.IssueLR.lrissuelist = result;
                $scope.IssueLR.bookingofficeid = result[0].transportid;
                $scope.IssueLR.fromlocationid = result[0].fromlocationid;
                $scope.IssueLR.receivinglocationid = result[0].receivinglocationid;
                $scope.IssueLR.companyid = result[0].lrcompanyid;
                $scope.IssueLR.noofbundels = 0;
                $scope.IssueLR.frieghtcharges = 0;
                $scope.IssueLR.noofpieces = 0;
                $scope.IssueLR.noofboxes = 0;
                $scope.IssueLR.weight = 0;

                $.each(result, function (k, v) {
                    $scope.IssueLR.noofbundels += v.noofbundels;
                    $scope.IssueLR.frieghtcharges += v.totalamount;
                    $scope.IssueLR.noofpieces += v.noofpieces;
                    $scope.IssueLR.noofboxes += v.noofboxes;
                    $scope.IssueLR.weight += v.weight;
                });
                $scope.ConfigureGrid();
                $scope.igrid.UpdateData($scope.IssueLR.lrissuelist, '_id');

            }).error(function (result) {
            });
        }
        else if (typeof ($location.search().id) != 'undefined' &&
            $location.search().id.length > 0) {
            request = {
                "Item": { 'id': $location.search().id },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LrissueService/Search",
                data: srequest
            }).success(function (result) {
                if (result.length == 1) {
                    $scope.IssueLR = result[0];
                }
                else {
                    $scope.IssueLR = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                $scope.ConfigureGrid();
                $scope.igrid.UpdateData($scope.IssueLR.lrissuelist, '_id');
            }).error(function (result) {
            });
        }
        else {
            $timeout(function () {
                $scope.IssueLR = (JSON.parse(JSON.stringify($scope.Entity)));
                $scope.ConfigureGrid();
                //$scope.setFocusfirst();
            }, 10);
        }

    }]);

