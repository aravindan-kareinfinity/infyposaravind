var app = angular.module('InfinityLab');

app.controller('ExpensesController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog', 'cntxService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, cntxService) {
        $scope.ExpensesItem = '';

        $scope.SearchRef = restService.SearchReference;

        $scope.ModeOfPayment = [{ 'id': 1, 'name': 'Check' }, { 'id': 2, 'name': 'Cash' }];

        $scope.ExpensesTypes = restService.Get('ReferencelistService/EXPENSESTYPE', true).then(function (results) {
            $scope.ExpensesTypes = results;
        });

        $scope.VendorTypes = restService.Get('ReferencelistService/VENDOR', true).then(function (results) {
            $scope.VendorTypes = results;
        });

        $scope.SearchResult = [];

        $scope.EditExpensesList = function (item) {


            $scope.EditItemList = item;

            $scope.ExpensesOpen();
            $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
        }

        $scope.AddExpensesOpen = function () {
            $scope.EditItemList = null;
            $scope.ExpensesOpen();
        }

        $scope.RemoveExpensesList = function (item) {


            $.confirm({
                title: 'Delete',
                content: 'Do you want to delete ' + $scope.SearchRef(item.expensestypeid, $scope.ExpensesTypes) + "?",
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
                                url: "ExpensesService/Delete",
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


      

        $scope.ExpensesOpen = function () {

            var item = { ExpensesItem: $scope.ExpensesItem, EditItem: $scope.EditItemList };

            var dialog = ngDialog.open({
                template: 'ExpensesPlan',
                data: [$scope, item],
                width: '1000px',
                controller: ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout',
                    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {

                    $scope.SearchRef = restService.SearchReference;

                    $scope.ExpensesTypes = restService.Get('ReferencelistService/EXPENSESTYPE', true).then(function (results) {
                        $scope.ExpensesTypes = results;
                    });

                    $scope.VendorTypes = restService.Get('ReferencelistService/VENDOR', true).then(function (results) {
                        $scope.VendorTypes = results;
                    });

                    $scope.Title = $scope.ngDialogData[1].ExpensesItem.name;

                    $scope.ItemId = $scope.ngDialogData[1].ExpensesItem.id;

                  
                    $scope.Expenses = $scope.ngDialogData[1].EditItem;



                
                    $scope.ModeOfPayment = [{ 'id': 1, 'name': 'Check' }, { 'id': 2, 'name': 'Cash' }];

                    
                    $scope.CheckPayment = function () {

                        if ($scope.Expenses.modeofpaymentid == 1) {

                            $scope.isBank = true;
                        }
                        else {
                            $scope.isBank = false;
                        }

                    }
                   

                    $scope.ExpensesList = [];
                   
                    $scope.Entity = {
                        "id": 0,
                        "expensestypeid": "",
                        "vendortypeid": "",
                        "expensesdate": new Date(),
                        "modeofpaymentid": "",
                        "amount": "",
                        "note": "",
                        "bankname":"",
                        "isactive": true,
                        "createdby": 0,
                        "modifiedby": 0,
                        "version": 1,
                        "createdon": "",
                        "modifiedon": ""
                    };

                  

                    if ($scope.Expenses == null)

                        $scope.Expenses = (JSON.parse(JSON.stringify($scope.Entity)));


                    $scope.Clear = function () {
                        $scope.Expenses = (JSON.parse(JSON.stringify($scope.Entity)));
                      

                    }
                    $scope.AddExpenses = function (callback) {

                            if ($scope.Expenses.vendortypeid == null || $scope.Expenses.vendortypeid == 0) {
                                $scope['setFocusfirst']();
                                $rootScope.Toaster.Error("Please Select Vendor");
                                return false;;
                            }
                            if ($scope.Expenses.expensesdate == undefined || $scope.Expenses.expensesdate == null) {
                                $scope['setFocusExpenDate']();
                                $rootScope.Toaster.Error("Please Select Expenses Date");
                                return false;;
                            }
                            if ($scope.Expenses.modeofpaymentid == null || $scope.Expenses.modeofpaymentid == 0) {
                                $scope['setFocusmodeofpaymentid']();
                                $rootScope.Toaster.Error("Please Select Mode Of Payment");
                                return false;;
                            }

                            if ($scope.Expenses.amount == null || $scope.Expenses.amount == 0) {
                                $scope['setFocusamount']();
                                $rootScope.Toaster.Error("Fill Out Amount");
                                return false;;
                            }
                    

                      

                        $scope.Expenses.expensestypeid = $scope.ItemId;

                        request = {
                            "Item": $scope.Expenses,
                            "securitytoken": "String content"
                        }

                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "ExpensesService/Save",
                            data: srequest
                        }).success(function (result) {

                            if (result != null)
                                $scope.ExpensesList.push(result);
                            
                            $scope.Expenses = (JSON.parse(JSON.stringify($scope.Entity)));

                            if (typeof (callback) != 'undefined')
                            {
                                callback(result);
                            }
                        }).error(function (result) {
                        });
                        
                    }
                    $scope.RemoveExpenses = function (item) {

                        $.confirm({
                            title: 'Delete',
                            content: 'Do you want to delete ' + $scope.SearchRef(item.expensestypeid, $scope.ExpensesTypes) + "?",
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
                                            url: "ExpensesService/Delete",
                                            data: srequest
                                        }).success(function (result) {

                                            if (result) {
                                                $scope.ErrorMessage = "Successfully deleted";
                                             $scope.ExpensesList.splice($scope.ExpensesList.indexOf(item), 1);
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

                    $scope.EditExpenses = function (item) {

                        if (item != null) {
                            $scope.Expenses = item;

                            $scope.ExpensesList.splice($scope.ExpensesList.indexOf(item),1)

                        }
                    }



                    $scope.SaveExpenses = function () {


                           if ($scope.ExpensesList != null && $scope.ExpensesList != 0) {
                               $scope.closeThisDialog($scope.ExpensesList);
                               $scope.Expenses = (JSON.parse(JSON.stringify($scope.Entity)));
                            }

                           else  if ($scope.Expenses != null && $scope.Expenses != 0) {


                            if ($scope.Expenses.vendortypeid != null && $scope.Expenses.vendortypeid != 0 || $scope.Expenses.expensesdate != null && $scope.Expenses.expensesdate != undefined || $scope.Expenses.amount != null && $scope.Expenses.expensesdate != undefined) {

                                $.confirm({
                                    title: 'Delete',
                                    content: 'Do you want to Save ' + $scope.Title + "?",
                                    confirmButton: 'Yes',
                                    confirmButtonClass: 'btn-info',
                                    icon: 'fa fa-question-circle',
                                    animation: 'scale',
                                    animationClose: 'top',
                                    opacity: 0.5,
                                    buttons: {
                                        formSubmit: {
                                            text: 'Save',
                                            btnClass: 'btn-blue',
                                            action: function () {
                                                $scope.AddExpenses(function (item) {
                                                    $scope.closeThisDialog($scope.ExpensesList);
                                                    $scope.Expenses = (JSON.parse(JSON.stringify($scope.Entity)));
                                                });
                                            }

                                        },
                                        cancel: function () {
                                            $scope.closeThisDialog();
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
                        }

                      

                     

                    };

                    $scope.CloseExpenses = function () {
                        $scope.closeThisDialog();
                    };
                }]
            });

            dialog.closePromise.then(function (data) {
                if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                    $.each(data.value, function (k, v) {

                        $scope.SearchResult.push(v);
                    })

                }
            });

        };
        $scope.SearchCondiction = {

            "vendortypeid": "",
            "expensesdate": "",
            "expensestypeid":""
        };

        $scope.Search = function (item) {

            if (typeof (item) != 'undefined') {

                $scope.ExpensesItem = item;
                $scope.SearchCondiction.expensestypeid = item.id;
               

            }

           
            request = {
                "Item": $scope.SearchCondiction,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ExpensesService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;


            }).error(function (result) {
            });
        }

    }]);