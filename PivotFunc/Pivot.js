
<html>
<p>Pivot Table</p>
<div id="pivot_tbl"><div>
</html>

<script>
  Array.prototype.sum = function (prop) {
        var result = 0
        for ( var i = 0, _len = this.length; i < _len; i++ ) {
          if(!isNaN(this[i][prop]))
          {
            result += this[i][prop]
          }
        }
        return result;
    }

  Array.prototype.avg = function (prop) {
        var result = 0
        for ( var i = 0, _len = this.length; i < _len; i++ ) {
          if(!isNaN(this[i][prop]))
          {
            result += this[i][prop]
          }
        }
        if(this.length>0)
        {return (result / this.length).toFixed(2)}
        else
        {return 0;}
    }   
  /*  
    Cal Func Code: 
    0. Count
    1. Sum
    2. Avg
    3. Max
    4. Min
  */

function Pivot(rawData,columnX,columnY_List,sortX_Asc,sortY_Asc,isTotalLeft,Calfunc,CalCol)
{
  //convert json to Array: [0] is the column array, from [1] is the data -result is the converted data array

  //column list of rawData
  var columns = Object.keys(rawData[0]);
  // final table
  var Result=[];
  //distinct X axis values
  let X_axis_values=[];

  //key-value pair of Y axis
  //key is column name
  //value is distinct column value
  let Y_axis_Pairs=[];

  //converted data
  let data = rawData.map(function(obj) {
    return columns.map(function(key) {
      return obj[key];
    });
  });
  data.unshift(columns);

  //fill X_axis_values
  let columnXValue = [];
  data.forEach(x => {if(data.indexOf(x)!=0) {columnXValue.push(x[columns.indexOf(columnX)])}});
  columnXValue = SortDistinctValue(sortX_Asc,columnXValue);

  //fill Y_axis_Pairs
  for(let y=0; y<columnY_List.length; y++)
  {
    let _columnName=columnY_List[y];
    
    let _columnValue=(function()
    {        
      let _index=columns.indexOf(_columnName);
      let columnvalue=[];
      data.forEach(x=> {if(data.indexOf(x)!=0) {columnvalue.push(x[_index])}})
      
      return SortDistinctValue(sortY_Asc,columnvalue);
    })();  
    let pair={columnName:_columnName,columnValues:_columnValue};
    Y_axis_Pairs.push(pair);
  }

  //add first row - Columns
  let FirstRow = [];
  columnY_List.forEach(x => {FirstRow.push(x);})
  columnXValue.forEach(x => {FirstRow.push(x);})

  isTotalLeft? FirstRow.splice(columnY_List.length,0,"Total") : FirstRow.push("Total");

  Result.push(FirstRow);

  //add value rows
  //set the y-axis pivot values
  let pivot_list=Axis_Value_List(Y_axis_Pairs,0); 
  
  //add cell values then add rows
  for(let i = 0; i < pivot_list.length; i++)
  {
    let CellRow=[];
    let TotalCell=0;//the total cell value

    //push axis values
    for(let j = 0; j < pivot_list[i].length; j++)
    {
      CellRow.push(pivot_list[i][j]);
    }

    //push cell value  
    for(let k = 0; k < columnXValue.length; k++)
    {
      let filter=[] //key is column name (index), value is column value
      //y-axis filters
      for(let l=0;l< columnY_List.length;l++)
      {
        filter.push({[`${columnY_List[l]}`] : pivot_list[i][l]});
      }
      //x-axis filter
      filter.push({[`${columnX}`] : columnXValue[k]});
      
      let rows = rawData.filter(function(row)
      {
        for(let m=0; m < filter.length; m++)
        {       
          if(row[Object.keys(filter[m])[0]] === undefined || Object.values(filter[m])[0] != row[Object.keys(filter[m])[0]])
          {return false;}
        }      
        return true;
      });

      //calculate these rows if they exist
      let Cal_result= "-"
      if(rows.length > 0)
      {
        switch(Calfunc)
        {
            /*  
              Cal Func Code: 
              0. Count
              1. Sum
              2. Avg
              3. Max
              4. Min
            */
          case 0:
            Cal_result = rows.length;
            TotalCell +=Cal_result;
          break;
          case 1:
            if(CalCol != "" && CalCol != undefined)
            {
              Cal_result = rows.sum(CalCol);
              TotalCell +=Cal_result;
            }
          break;
          case 2:
            if(CalCol != "" && CalCol != undefined)
            {
              Cal_result = rows.avg(CalCol);
            }
          break;
          case 3:
            if(CalCol != "" && CalCol != undefined)
            {
              Cal_result = Math.max.apply(Math,rows.map(function(o){ return o[CalCol];}))
            }
          break;
          case 4:
            if(CalCol != "" && CalCol != undefined)
            {
              Cal_result = Math.min.apply(Math,rows.map(function(o){ return o[CalCol];}))
            }
          break;
        }
      }  
      CellRow.push(Cal_result);
    }

    //push total value to the row base on where the total will be
    isTotalLeft? CellRow.splice(pivot_list[0].length,0,TotalCell) : CellRow.push(TotalCell);
    //CellRow.splice((isTotalLeft ? pivot_list[0].length : (pivot_list[0].length + columnXValue.length-1)),0,TotalCell);    
    Result.push(CellRow);
  }
  
  //add total last row
  let TotalRow =[];
  for(let i=0; i< Y_axis_Pairs.length-1;i++){TotalRow.push("-");/*null values to fill the bottom cell under y-axis-column value*/}
  TotalRow.push("Total");
  
  //fill total row
  for(let i =Y_axis_Pairs.length;i < Result[0].length;i++)
  {
    let total_result =0;
    
    for(let j = 1 ; j < Result.length;j++)
    {
      //let testoneeee=Result[j][i];
      
      total_result += ((isNaN(Result[j][i]))? 0 : Result[j][i]);
    }

    TotalRow.push(total_result);
  }

  Result.push(TotalRow);
  return Result;
}
let pivot_tbl = Pivot(sourceData,"Aging_By_Received",["Status"],true,false,false,0);

RenderPivotTbl(pivot_tbl);

function RenderPivotTbl(tbl_array)
{
  var html = "<table border='1|1'>";
  for (var i = 0; i < tbl_array.length; i++) {
      html+="<tr>";

      for(var j=0; j < tbl_array[i].length;j++)
      {
        html+="<td>"+tbl_array[i][j]+"</td>";    
      }

      html+="</tr>";

  }
  html+="</table>";
  document.getElementById("pivot_tbl").innerHTML = html;
}
function SortDistinctValue(sort,_array)
{
  if(!_array.some(isNaN))
    {
      //if all numbers, sort as numbers
      return sort 
      ? _array.filter((v, i, a) => a.indexOf(v) === i).sort(function(a,b) { return a - b; }) 
      : _array.filter((v, i, a) => a.indexOf(v) === i).sort(function(a,b) { return b - a; });       
    }
    else
    {
      return sort
      ? _array.filter((v, i, a) => a.indexOf(v) === i).sort() 
      : _array.filter((v, i, a) => a.indexOf(v) === i).reverse();
    }
}

function Axis_Value_List(Y_Axis_Pairs,i)
{
  
  let _result=[];

  if(i> Y_Axis_Pairs.length-1)
  {    
    for(let j=0;j<Y_Axis_Pairs[Y_Axis_Pairs.length-1].columnValues.length;j++)
    {
      let temp_result=[];
      temp_result.push(Y_Axis_Pairs[Y_Axis_Pairs.length-1].columnValues[j])      
      _result.push(temp_result);       
    }       
    return _result;
  }
  else
  {
    let sample =Axis_Value_List(Y_Axis_Pairs,i+1);
    
    if(i > 0)
    {     
      for(let j=0; j< Y_Axis_Pairs[i-1].columnValues.length; j++)
      {
        let _result_replica = DeepCopy(sample);
        
        for(let k=0;k<_result_replica.length;k++)
        {          
          _result_replica[k].unshift(Y_Axis_Pairs[i-1].columnValues[j]);
          
        }
        
        _result=_result.concat(_result_replica);                   
      }
      
    }
    else
    {
      _result=sample;
    }
    
    return _result;   
  }
}

function DeepCopy(source)
{
  let result =[];

  for(let i=0; i < source.length; i++)
  {
    let _list=[];

    for(let j=0; j < source[i].length;j++)
    {
      _list.push(source[i][j]);
    }

    result.push(_list);
  }

  return result;
}
</script>
