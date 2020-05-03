import React from 'react';
import styles from './styles.module.css'
import { MAX_HEIGHT, MAX_WIDTH } from "../../consts/sizes";


export default function Person({ person, onClick }) {
    const x = person.position.x / MAX_WIDTH * 100;
    const y = person.position.y / MAX_HEIGHT * 100;
    const isSick = person.isSick;
    return (
        <div
            className={ styles.root }
            style={
                { 
                    left: `${ x }%`,
                    top: `${ y }%`,
                    backgroundColor: `${isSick ? "red" : "#d2b1e7"}`   
                }
            }
            onClick={ () => onClick(person.id) }
        />
    );
}
