/***
 * Contains basic SlickGrid formatters.
 * 
 * NOTE:  These are merely examples.  You will most likely need to implement something more
 *        robust/extensible/localizable/etc. for your use!
 * 
 * @module Formatters
 * @namespace Slick
 */

(function ($) {
  // register namespace
  $.extend(true, window, {
    "Slick": {
      "Formatters": {
        "PercentComplete": PercentCompleteFormatter,
        "PercentCompleteBar": PercentCompleteBarFormatter,
        "YesNo": YesNoFormatter,
        "Checkmark": CheckmarkFormatter,
        "NumberFormatter": NumberFormatter,
        "IndianNumberFormatter": IndianNumberFormatter,
        "DateFormatter": DateFormatter
      }
    }
  });

  function PercentCompleteFormatter(row, cell, value, columnDef, dataContext) {
    if (value == null || value === "") {
      return "-";
    } else if (value < 50) {
      return "<span style='color:red;font-weight:bold;'>" + value + "%</span>";
    } else {
      return "<span style='color:green'>" + value + "%</span>";
    }
  }

  function PercentCompleteBarFormatter(row, cell, value, columnDef, dataContext) {
    if (value == null || value === "") {
      return "";
    }

    var color;

    if (value < 30) {
      color = "red";
    } else if (value < 70) {
      color = "silver";
    } else {
      color = "green";
    }

    return "<span class='percent-complete-bar' style='background:" + color + ";width:" + value + "%'></span>";
  }

    function NumberFormatter(row, cell, value, columnDef, dataContext) {
        value = parseFloat(value);
        var svalue = value.toFixed(columnDef.decimal);

        var values = svalue.toString().split('.');

        var leftvalue = values[0].toString().replace(/(\d)(?=(\d\d)+\d$)/g, "$1,");
        if (values.length == 2)
            return leftvalue + "." + values[1];
        return leftvalue;
      //return jQuery.number(value, columnDef.decimal)
  }


    //function IndianNumberFormatter(value, decimalpoint) {
    //    value = parseFloat(value);
    //    var svalue = value.toFixed(decimalpoint);

    //    var values = svalue.toString().split('.');

    //    var leftvalue = values[0].toString().replace(/(\d)(?=(\d\d)+\d$)/g, "$1,");
    //    if (values.length == 2)
    //        return leftvalue + "." + values[1];
    //    return leftvalue;
    //}

    function IndianNumberFormatter(row, cell, value, columnDef, dataContext) {
        value = parseFloat(value);
        if (isNaN(value)) value = 0;
      var svalue = value.toFixed(columnDef.decimal);

      var values = svalue.toString().split('.');

      var leftvalue = values[0].toString().replace(/(\d)(?=(\d\d)+\d$)/g, "$1,");
      if (values.length == 2)
          return leftvalue + "." + values[1];
      return leftvalue;

      //var newValue = [];

      //var intPart = values[0].split("").reverse();
      //var index = 0;
      //while (intPart.length > 0) {
      //    if (index == 3 || (index > 3 && index % 2 == 1))
      //        newValue.push(",");
      //    newValue.push(intPart.shift());
      //    index++;
      //}

      //newValue.reverse();
      //if (values.length == 0)
      //    return newValue.join("");
      //else if (values.length==2)
      //    return newValue.join("") + "." + values[1];
      //return newValue.join("")
  }

  function YesNoFormatter(row, cell, value, columnDef, dataContext) {
    return value ? "Yes" : "No";
  }

    function DateFormatter(row, cell, value, columnDef, dataContext) {
        if (value == undefined || value == null) return '';
      var result = jQuery.format.date(value, columnDef.format);
        if (result == '0001-01-01' || result == '01-01-0001' || result == '01-01-1901') return '';
      return result;
  }

  function CheckmarkFormatter(row, cell, value, columnDef, dataContext) {
      return value ? "<img class='slick-check-tick'>" : "";
  }
})(jQuery);
