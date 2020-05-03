import React from 'react';
import styles from './styles.module.css'
import { MAX_HEIGHT, MAX_WIDTH } from "../../consts/sizes";


const HEALTH_COLORS = {
    Healthy: "#d2b1e7",
    Sick: "red",
    Dead: "black"
}

export default function Person({ person, onClick }) {
    const x = person.position.x / MAX_WIDTH * 100;
    const y = person.position.y / MAX_HEIGHT * 100;
    return (
        <div
            className={ styles.root }
            style={
                { 
                    left: `${ x }%`,
                    top: `${ y }%`,
                    backgroundColor: HEALTH_COLORS[person.health]  
                }
            }
            onClick={ () => onClick(person.id) }
        />
    );
}
