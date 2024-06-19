import React from 'react';
import {useState, useEffect} from 'react';
import './StatusPage.css';
import { ColorRing } from 'react-loader-spinner';

export default function Uspeh(props)
{
    const [textal, setTextAl] = useState(props.textal);
    const [pleft,setPLeft] = useState(props.pleft);
    const [pright,setPRight] = useState(props.pright);
    useEffect(() => {
        if(!props.textal)
        {
            setTextAl("center");
        }
        else 
        {
            setTextAl(props.textal);
        }
        if(!props.pleft)
        {
            setPLeft("0px");
        }
        else 
        {
            setPLeft(props.pleft);
        }
        if(!props.pright)
        {
            setPRight("0px");
        }
        else 
        {
            setPRight(props.pright);
        }
    },[props.textal,textal,props.pleft,pleft,props.pright,pright]);

    return(
        <div className = "wrapper">
            <h1>{props.heading}</h1>
            <p style = {{ paddingLeft: pleft, paddingRight: pright,paddingBottom: "20px", textAlign: textal}}>{props.mesg}</p>
            {props.spinner && 
            <div className = "spinner-box">
                <ColorRing  
                visible={true}
                height="60"
                width="60"
                ariaLabel="color-ring-loading"
                wrapperStyle={{}}
                wrapperClass="color-ring-wrapper"
                colors={['#000000','#3376BA','#63A3E4','#3376BA','#000000']}/>
            </div>}
            {props.hasButton && <div className = "button-box">
                    <button onClick={props.fnc}>{props.btnText}</button>
            </div>}
        </div>
    );
}